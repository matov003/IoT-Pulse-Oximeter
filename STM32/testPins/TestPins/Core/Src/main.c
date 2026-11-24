/* USER CODE BEGIN Header */
/**
  ******************************************************************************
  * @file           : main.c
  * @brief          : Main program body
  ******************************************************************************
  * @attention
  *
  * Copyright (c) 2026 STMicroelectronics.
  * All rights reserved.
  *
  * This software is licensed under terms that can be found in the LICENSE file
  * in the root directory of this software component.
  * If no LICENSE file comes with this software, it is provided AS-IS.
  *
  ******************************************************************************
  */
/* USER CODE END Header */
/* Includes ------------------------------------------------------------------*/
#include "main.h"

/* Private includes ----------------------------------------------------------*/
/* USER CODE BEGIN Includes */
#include <string.h>
#include <stdarg.h>
#include <stdint.h>
#include <stdbool.h>
#include <math.h>

#include "ssd1306.h"
#include "fonts.h"
/* USER CODE END Includes */

/* Private typedef -----------------------------------------------------------*/
/* USER CODE BEGIN PTD */

/* USER CODE END PTD */

/* Private define ------------------------------------------------------------*/
/* USER CODE BEGIN PD */
#define WIFI_NAME      "***"        
#define WIFI_PASSWORD  "***" 

#define SERVER_IP      "172.20.10.2"   
#define SERVER_PORT    5000
#define SEND_PERIOD_MS 1000            	
#define USE_WATCHDOG   1               
#define DEBUG_OLED_DC  1               
#define AUTO_START     0              

#define START_PERCENT     50           
                                       
#define RAW_SATURATION    4000U       
#define Q_SIZE            1024U        

#ifndef LED_RED_Pin
#define LED_RED_Pin    GPIO_PIN_8      
#endif
#ifndef LED_IR_Pin
#define LED_IR_Pin     GPIO_PIN_9      
#endif
#define LED_RED_PIN    ((uint32_t)LED_RED_Pin)
#define LED_IR_PIN     ((uint32_t)LED_IR_Pin)

#define LD2_ON()       (GPIOA->BSRR = (uint32_t)GPIO_PIN_5)           /* zelena LED na Nucleu (PA5) */
#define LD2_OFF()      (GPIOA->BSRR = (uint32_t)GPIO_PIN_5 << 16)
/* USER CODE END PD */

/* Private macro -------------------------------------------------------------*/
/* USER CODE BEGIN PM */

/* USER CODE END PM */

/* Private variables ---------------------------------------------------------*/
ADC_HandleTypeDef hadc1;

I2C_HandleTypeDef hi2c2;

TIM_HandleTypeDef htim1;

UART_HandleTypeDef huart1;

/* USER CODE BEGIN PV */
/* trajanja u impulsima TIM1 (racunaju se iz SystemCoreClock u Acq_Start) */
static uint32_t T_PHASE, T_MIN, T_START, T_TRIG;
/* USER CODE END PV */

/* Private function prototypes -----------------------------------------------*/
void SystemClock_Config(void);
static void MX_GPIO_Init(void);
static void MX_TIM1_Init(void);
static void MX_ADC1_Init(void);
static void MX_I2C2_Init(void);
static void MX_USART1_UART_Init(void);
/* USER CODE BEGIN PFP */

/* USER CODE END PFP */

/* Private user code ---------------------------------------------------------*/
/* USER CODE BEGIN 0 */

static int sfmt(char *dst, int cap, const char *fmt, ...)
{
    va_list ap;
    int n = 0;
    va_start(ap, fmt);
#define SFMT_PUT(c) do { if (n < cap - 1) dst[n] = (c); n++; } while (0)
    while (*fmt)
    {
        if (*fmt != '%') { SFMT_PUT(*fmt); fmt++; continue; }
        fmt++;

        int width = 0, prec = -1, lng = 0, len = 0;
        char tmp[12];
        const char *s = tmp;

        while (*fmt >= '0' && *fmt <= '9') { width = width * 10 + (*fmt - '0'); fmt++; }
        if (*fmt == '.')
        {
            fmt++; prec = 0;
            while (*fmt >= '0' && *fmt <= '9') { prec = prec * 10 + (*fmt - '0'); fmt++; }
        }
        if (*fmt == 'l') { lng = 1; fmt++; }
        char t = *fmt;
        if (t) fmt++;

        if (t == 's')
        {
            s = va_arg(ap, const char *);
            while (s[len] && (prec < 0 || len < prec)) len++;
        }
        else if (t == 'd' || t == 'u')
        {
            unsigned long v;
            int neg = 0, k = 0;
            char rev[12];
            if (t == 'd')
            {
                long sv = lng ? va_arg(ap, long) : (long)va_arg(ap, int);
                if (sv < 0) { neg = 1; v = 0UL - (unsigned long)sv; } else v = (unsigned long)sv;
            }
            else v = lng ? va_arg(ap, unsigned long) : (unsigned long)va_arg(ap, unsigned int);
            do { rev[k++] = (char)('0' + (int)(v % 10UL)); v /= 10UL; } while (v);
            if (neg) rev[k++] = '-';
            while (k) tmp[len++] = rev[--k];
        }
        else if (t) { tmp[0] = t; len = 1; }          

        for (int p = len; p < width; p++) SFMT_PUT(' ');
        for (int p = 0; p < len; p++)     SFMT_PUT(s[p]);
    }
#undef SFMT_PUT
    if (cap > 0) dst[n < cap ? n : cap - 1] = 0;
    va_end(ap);
    return n;
}

#define FS                200.0f    /* izlazna frekvencija odabiranja (Hz)        */
#define ALPHA_DC          0.0093f   /* IIR koeficijent za DC (fc ~ 0,3 Hz)        */
#define SPO2_A            110.0f    /* SpO2 = SPO2_A - SPO2_B * R (kalibracija prema referentnom oksimetru) */
#define SPO2_B            25.0f
#define DC_LOW            500.0f    /* ispod -> signal slab (AGC povecava sirinu) */
#define DC_HIGH           1600.0f   /* iznad -> signal jak (AGC smanjuje sirinu)  */
#define FINGER_IR_MIN     100.0f    /* IR (RED-AMB) ispod -> nema prsta           */
#define AVG_BEATS         4         /* usrednjavanje rezultata po 4 otkucaja      */
#define BEAT_MIN_SAMPLES  60        /* 300 ms izmedju dva otkucaja                */
#define BEAT_MAX_SAMPLES  400       /* 2 s = 30 otkucaja/min                      */
#define BEAT_LOST_SAMPLES 800       /* 4 s bez otkucaja -> rezultat nije pouzdan  */
#define SETTLE_SAMPLES    600       /* 3 s cekanja posle promene sirine (200 Hz)  */


#define PWM_MIN_TICKS     T_MIN     /* 100 us   */
#define PWM_MAX_TICKS     T_PHASE   /* 312,5 us */
#define PWM_INIT_TICKS    T_START   /* 50 % faze */

/* Elipticni nisko-propusni filter 5. reda: fc = 5 Hz, fs = 200 Hz, talasanje u propusnom opsegu
   0,5 dB, slabljenje u nepropusnom opsegu 40 dB. Tri sekcije drugog reda {b0,b1,b2,a1,a2}
   (prva je zapravo prvog reda). Projektovano sa scipy.signal.ellip(5, 0.5, 40, 5, fs=200, output='sos'). */
static const float ELL[3][5] = {
    { 0.0037368455f,  0.0037368455f, 0.0f,           -0.9286583105f, 0.0f           },
    { 1.0f,          -1.9143121581f, 1.0f,           -1.9020096863f, 0.9171306429f  },
    { 1.0f,          -1.9577637600f, 1.0f,           -1.9544652492f, 0.9795389129f  }
};

/* jedan odbirak kroz filter; z[6] = stanja tri sekcije (direktna forma II, transponovana) */
static float Ellip5(float *z, float x)
{
    for (uint8_t i = 0; i < 3U; i++)
    {
        float y = ELL[i][0] * x + z[2U * i];
        z[2U * i]      = ELL[i][1] * x - ELL[i][3] * y + z[2U * i + 1U];
        z[2U * i + 1U] = ELL[i][2] * x - ELL[i][4] * y;
        x = y;
    }
    return x;
}

/* medijana n (1..4) intervala u odbircima */
static float Median_Interval(const uint16_t *v, uint8_t n)
{
    uint16_t t[AVG_BEATS];
    for (uint8_t i = 0; i < n; i++) t[i] = v[i];
    for (uint8_t i = 1; i < n; i++)                      /* sortiranje umetanjem */
    {
        uint16_t x = t[i];
        int8_t j = (int8_t)i - 1;
        while (j >= 0 && t[j] > x) { t[j + 1] = t[j]; j--; }
        t[j + 1] = x;
    }
    return (n & 1U) ? (float)t[n / 2U] : 0.5f * ((float)t[n / 2U - 1U] + (float)t[n / 2U]);
}

typedef struct {
   
    uint32_t sum_red, sum_ir, sum_amb;
    uint8_t  avg_count;

    
    uint16_t pwm_red;
    uint16_t pwm_ir;
    uint32_t agc_hold_timer;     

    
    bool  filt_init;
    float dc_red, dc_ir;
    float ac_red, ac_ir;
    float ac_ir_filt;

    
    uint32_t sample_counter;
    uint32_t last_peak_sample;
    float    ac_ir_sq_sum, ac_red_sq_sum;
    uint32_t pulse_sample_count;
    float    env;                 
    bool     armed;               
    bool     have_ref;            
    uint16_t int_buf[AVG_BEATS];
    float    spo2_buf[AVG_BEATS];         /* R po otkucaju (iz proseka se racuna SpO2) */
    float    zr[6], zi[6];                /* stanja eliptickog filtra: crveni, IR */
    uint8_t  int_n, spo2_n, int_idx, spo2_idx;

    /* izlaz */
    float spo2;
    float r_mean;                 /* prosecno R poslednjih otkucaja */
    float heart_rate;
    bool  finger_present;
    bool  reliable_result;
} Oksimetar;

static Oksimetar my_oximeter;

static void Beat_Reset(Oksimetar *dev)
{
    dev->ac_ir_sq_sum = dev->ac_red_sq_sum = 0.0f;
    dev->pulse_sample_count = 0;
    dev->last_peak_sample = dev->sample_counter;
    dev->armed = false;
    dev->have_ref = false;
    dev->int_n = dev->spo2_n = dev->int_idx = dev->spo2_idx = 0;
    dev->reliable_result = false;
}

static void Oksimetar_Init(Oksimetar *dev)
{
    dev->sum_red = dev->sum_ir = dev->sum_amb = 0;
    dev->avg_count = 0;

    dev->pwm_red = (uint16_t)PWM_INIT_TICKS;
    dev->pwm_ir  = (uint16_t)PWM_INIT_TICKS;
    dev->agc_hold_timer = 0;

    dev->filt_init = false;
    dev->dc_red = dev->dc_ir = 0.0f;
    dev->ac_red = dev->ac_ir = 0.0f;
    dev->ac_ir_filt = 0.0f;
    dev->env = 0.0f;

    dev->sample_counter = 0;
    Beat_Reset(dev);

    dev->spo2 = 0.0f;
    dev->r_mean = 0.0f;
    dev->heart_rate = 0.0f;
    dev->finger_present = false;
}


static void Oksimetar_Process200Hz(Oksimetar *dev)
{
    dev->sample_counter++;
    dev->pulse_sample_count++;

 
    dev->ac_ir_sq_sum  += dev->ac_ir  * dev->ac_ir;
    dev->ac_red_sq_sum += dev->ac_red * dev->ac_red;

  
    float a = dev->ac_ir_filt < 0.0f ? -dev->ac_ir_filt : dev->ac_ir_filt;
    dev->env = (a > dev->env) ? a : dev->env * 0.998f;
    float hyst = 0.25f * dev->env;
    if (hyst < 1.5f) hyst = 1.5f;

    uint32_t since = dev->sample_counter - dev->last_peak_sample;

    if (dev->ac_ir_filt < -hyst)
    {
        dev->armed = true;
    }
    else if (dev->armed && dev->ac_ir_filt >= 0.0f)
    {
        dev->armed = false;                              /* svaki prolaz kroz nulu se trosi, i prerani (sum) */

        if (since >= BEAT_MIN_SAMPLES)                   /* ranije od 300 ms = sum, preskoci */
        {
            if (!dev->have_ref)
            {
                dev->have_ref = true;                    /* prvi otkucaj = samo referenca */
            }
            else if (since <= BEAT_MAX_SAMPLES && dev->pulse_sample_count > 10)
            {
                /* 1) interval -> puls: HR = 60 * fs / N, N = medijana poslednjih intervala (otporna na pojedinacne greske) */
                dev->int_buf[dev->int_idx] = (uint16_t)since;
                dev->int_idx = (uint8_t)((dev->int_idx + 1U) % AVG_BEATS);
                if (dev->int_n < AVG_BEATS) dev->int_n++;
                dev->heart_rate = 60.0f * FS / Median_Interval(dev->int_buf, dev->int_n);

                /* 2) RMS filtriranih AC komponenti, R = (AC_RED/DC_RED)/(AC_IR/DC_IR) */
                float ac_ir_rms  = sqrtf(dev->ac_ir_sq_sum  / (float)dev->pulse_sample_count);
                float ac_red_rms = sqrtf(dev->ac_red_sq_sum / (float)dev->pulse_sample_count);
                if (dev->dc_red > 1.0f && dev->dc_ir > 1.0f && ac_ir_rms > 1.0f)
                {
                    float r_ratio = (ac_red_rms / dev->dc_red) / (ac_ir_rms / dev->dc_ir);
                    if (r_ratio >= 0.1f && r_ratio <= 3.5f)
                    {
                        dev->spo2_buf[dev->spo2_idx] = r_ratio;
                        dev->spo2_idx = (uint8_t)((dev->spo2_idx + 1U) % AVG_BEATS);
                        if (dev->spo2_n < AVG_BEATS) dev->spo2_n++;
                    }
                }
                if (dev->spo2_n > 0 && dev->int_n >= 2)
                {
                    float sm = 0.0f;
                    for (uint8_t k = 0; k < dev->spo2_n; k++) sm += dev->spo2_buf[k];
                    dev->r_mean = sm / (float)dev->spo2_n;
                    float s = SPO2_A - SPO2_B * dev->r_mean;
                    if (s > 100.0f) s = 100.0f;
                    if (s < 0.0f)   s = 0.0f;
                    dev->spo2 = s;
                    dev->reliable_result = true;
                }
            }

            dev->last_peak_sample = dev->sample_counter;
            dev->ac_ir_sq_sum = dev->ac_red_sq_sum = 0.0f;
            dev->pulse_sample_count = 0;
        }
    }
    else if (since > BEAT_LOST_SAMPLES)
    {
        Beat_Reset(dev);                                 /* predugo bez otkucaja: trazi iznova */
    }
}

static void Oksimetar_ProcessMultiplexSample(Oksimetar *dev, uint16_t red, uint16_t ir, uint16_t amb)
{
    dev->sum_red += red;
    dev->sum_ir  += ir;
    dev->sum_amb += amb;
    dev->avg_count++;

    if (dev->avg_count < 4) return;                      

    float avg_red = (float)dev->sum_red / 4.0f;
    float avg_ir  = (float)dev->sum_ir  / 4.0f;
    float avg_amb = (float)dev->sum_amb / 4.0f;
    dev->sum_red = dev->sum_ir = dev->sum_amb = 0;
    dev->avg_count = 0;

    /* korekcija ambijentalnog svetla: izrazi (14) i (15) */
    float red_corr = avg_red - avg_amb;
    float ir_corr  = avg_ir  - avg_amb;
    if (red_corr < 0.0f) red_corr = 0.0f;
    if (ir_corr  < 0.0f) ir_corr  = 0.0f;

    /* prisustvo prsta */
    if (ir_corr > FINGER_IR_MIN)
    {
        dev->finger_present = true;
    }
    else
    {
        dev->finger_present = false;
        dev->filt_init = false;                          /* sledeci prst pocinje cist */
        dev->agc_hold_timer = 0;
        dev->pwm_red = (uint16_t)PWM_INIT_TICKS;
        dev->pwm_ir  = (uint16_t)PWM_INIT_TICKS;
        Beat_Reset(dev);
        return;
    }

    /* IIR: DC komponenta, a AC = signal - DC */
    if (!dev->filt_init)
    {
        dev->dc_red = red_corr;                          /* bez dugog prelaznog procesa od nule */
        dev->dc_ir  = ir_corr;
        dev->ac_ir_filt = 0.0f;
        memset(dev->zr, 0, sizeof(dev->zr));             /* stanja eliptickog filtra */
        memset(dev->zi, 0, sizeof(dev->zi));
        dev->filt_init = true;
    }
    dev->dc_red += ALPHA_DC * (red_corr - dev->dc_red);
    dev->dc_ir  += ALPHA_DC * (ir_corr  - dev->dc_ir);
    /* AC = (signal - DC) kroz eliptic LP filter 5. reda (fc = 5 Hz); isti filter za oba kanala */
    dev->ac_red = Ellip5(dev->zr, red_corr - dev->dc_red);
    dev->ac_ir  = Ellip5(dev->zi, ir_corr  - dev->dc_ir);
    dev->ac_ir_filt = dev->ac_ir;

    /* AGC: sirina PWM impulsa tako da DC ostane u opsegu 500 - 1600 */
    if (dev->agc_hold_timer > 0)
    {
        dev->agc_hold_timer--;
        if (dev->agc_hold_timer == 0) Beat_Reset(dev);   /* posle cekanja nov pocetak detekcije */
        return;                                          /* dok traje cekanje, SpO2 se ne racuna */
    }

    bool adjust = false;
    if (dev->dc_red < DC_LOW && dev->pwm_red < PWM_MAX_TICKS)       { dev->pwm_red = (uint16_t)(dev->pwm_red * 1.2f); adjust = true; }
    else if (dev->dc_red > DC_HIGH && dev->pwm_red > PWM_MIN_TICKS) { dev->pwm_red = (uint16_t)(dev->pwm_red * 0.8f); adjust = true; }

    if (dev->dc_ir < DC_LOW && dev->pwm_ir < PWM_MAX_TICKS)         { dev->pwm_ir = (uint16_t)(dev->pwm_ir * 1.2f);  adjust = true; }
    else if (dev->dc_ir > DC_HIGH && dev->pwm_ir > PWM_MIN_TICKS)   { dev->pwm_ir = (uint16_t)(dev->pwm_ir * 0.8f);  adjust = true; }

    if (dev->pwm_red > PWM_MAX_TICKS) dev->pwm_red = (uint16_t)PWM_MAX_TICKS;
    if (dev->pwm_red < PWM_MIN_TICKS) dev->pwm_red = (uint16_t)PWM_MIN_TICKS;
    if (dev->pwm_ir  > PWM_MAX_TICKS) dev->pwm_ir  = (uint16_t)PWM_MAX_TICKS;
    if (dev->pwm_ir  < PWM_MIN_TICKS) dev->pwm_ir  = (uint16_t)PWM_MIN_TICKS;

    if (adjust)
    {
        dev->agc_hold_timer = SETTLE_SAMPLES;            /* 3 s da se IIR stabilizuje */
        Beat_Reset(dev);
        return;
    }

    Oksimetar_Process200Hz(dev);                         /* puls i SpO2 */
}

typedef struct { uint16_t red, ir, amb; } Raw_t;

static volatile uint16_t adc_buf[4];     /* upisuje DMA: RED, IR, AMB, pauza */
static volatile uint8_t  g_phase;
static volatile uint8_t  g_leds_on;      /* 1 = LED dozvoljene (merenje je ukljuceno) */
static volatile uint8_t  g_measuring;    /* 1 = odbirci idu u red */
static Raw_t             q[Q_SIZE];
static volatile uint16_t q_head, q_tail;
static volatile uint16_t q_overflow;     /* izgubljeni odbirci (gledaj u debageru) */
static volatile uint16_t dma_resync;     /* mora da miruje */
static uint16_t          sat_hold;       /* > 0: nedavno je bilo zasicenje (odbrojava se po odbirku) */
static volatile uint8_t  system_active;  /* 1 = merenje ukljuceno (dugme B1) */

/* kraj ciklusa (DMA upisao 4 odbirka): ide u red, obradu radi glavni program */
static void Acq_ProcessCycle(void)
{
    if (!g_measuring) return;
    uint16_t n = (uint16_t)((q_head + 1U) & (Q_SIZE - 1U));
    if (n != q_tail)
    {
        q[q_head].red = adc_buf[0];
        q[q_head].ir  = adc_buf[1];
        q[q_head].amb = adc_buf[2];
        q_head = n;
    }
    else q_overflow++;
}

/* upis CCR (preload) za fazu ph: stupa na snagu na sledecem UPDATE-u */
static inline void Acq_ProgramPhase(uint8_t ph)
{
    uint32_t r = 0U, i = 0U, trig = T_TRIG;
    if (g_leds_on)
    {
        if (ph == 0U)      r = my_oximeter.pwm_red;      /* sirinu odredjuje AGC */
        else if (ph == 1U) i = my_oximeter.pwm_ir;
    }
    TIM1->CCR1 = r;
    TIM1->CCR2 = i;
    TIM1->CCR3 = trig;
}

/* pocetak svake faze */
void TIM1_UP_IRQHandler(void)
{
    if (TIM1->SR & TIM_SR_UIF)
    {
        TIM1->SR = ~TIM_SR_UIF;
        uint8_t ph = (uint8_t)((g_phase + 1U) & 3U);
        g_phase = ph;
        if (ph == 0U && DMA1_Channel1->CNDTR != 4U)       /* DMA nije na pocetku ciklusa */
        {
            DMA1_Channel1->CCR  &= ~DMA_CCR_EN;
            DMA1_Channel1->CNDTR = 4U;
            DMA1_Channel1->CCR  |= DMA_CCR_EN;
            dma_resync++;
        }
        Acq_ProgramPhase((uint8_t)((ph + 1U) & 3U));
    }
}

void DMA1_Channel1_IRQHandler(void)
{
    if (DMA1->ISR & DMA_ISR_TCIF1)
    {
        DMA1->IFCR = DMA_IFCR_CGIF1;
        Acq_ProcessCycle();
    }
}

/* TIM1 + ADC1 + DMA1 preko registara (ne zavisi od CubeMX vrednosti) i start PWM-a */
static void Acq_Start(void)
{
    T_PHASE  = SystemCoreClock / 3200UL;            /* 312,5 us */
    T_MIN    = SystemCoreClock / 10000UL;           /* 100 us   */
    T_START  = T_PHASE * START_PERCENT / 100UL;
    T_TRIG   = SystemCoreClock / 20000UL;           /*  50 us   */

    RCC->APB2ENR |= RCC_APB2ENR_ADC1EN | RCC_APB2ENR_IOPAEN | RCC_APB2ENR_IOPCEN
                  | RCC_APB2ENR_AFIOEN | RCC_APB2ENR_TIM1EN;
    RCC->AHBENR  |= RCC_AHBENR_DMA1EN;
    RCC->CFGR = (RCC->CFGR & ~RCC_CFGR_ADCPRE) | RCC_CFGR_ADCPRE_DIV6;   /* ADC takt < 14 MHz */
    (void)RCC->APB2ENR;
    AFIO->MAPR = (AFIO->MAPR & ~AFIO_MAPR_SWJ_CFG) | AFIO_MAPR_SWJ_CFG_JTAGDISABLE;   /* SWD ostaje ukljucen (PA13/PA14) */

    /* PA0 analogni ulaz; PA8/PA9 izlazi na 0 dok PWM ne krene (LED ugasene) */
    GPIOA->CRL &= ~(0xFUL << 0);
    GPIOA->BRR  = LED_RED_PIN | LED_IR_PIN;
    GPIOA->CRH  = (GPIOA->CRH & ~0xFFUL) | 0x33UL;

    
    GPIOC->CRH = (GPIOC->CRH & ~(0xFUL << 20)) | (0x8UL << 20);          /* PC13 ulaz sa pull-up/down */
    GPIOC->BSRR = (1UL << 13);                                           /* pull-up */
    AFIO->EXTICR[3] = (AFIO->EXTICR[3] & ~(0xFUL << 4)) | (0x2UL << 4);   /* EXTI13 = port C */
    EXTI->RTSR &= ~(1UL << 13);                                          /* ne reaguj na otpustanje dugmeta (CubeMX je ukljucio rastucu ivicu) */
    EXTI->FTSR |= (1UL << 13);
    EXTI->IMR  |= (1UL << 13);
    EXTI->PR    = (1UL << 13);

    /* ADC1: ukljuci, kalibracija, kanal 0, 71,5 ciklusa, 1 konverzija */
    ADC1->CR1 = 0U;
    ADC1->CR2 = ADC_CR2_ADON;
    for (volatile uint32_t k = 0; k < 400U; k++) { }
    ADC1->CR2 |= ADC_CR2_RSTCAL;  while (ADC1->CR2 & ADC_CR2_RSTCAL) { }
    ADC1->CR2 |= ADC_CR2_CAL;     while (ADC1->CR2 & ADC_CR2_CAL)    { }
    ADC1->SMPR2 = (ADC1->SMPR2 & ~ADC_SMPR2_SMP0) | (6UL << ADC_SMPR2_SMP0_Pos);
    ADC1->SQR1 = 0U;
    ADC1->SQR3 = 0U;

    /* DMA1 kanal 1: ADC1->DR -> adc_buf[4], kruzno, 16 bita, prekid na kraju */
    DMA1_Channel1->CCR   = 0U;
    DMA1->IFCR           = DMA_IFCR_CGIF1;
    DMA1_Channel1->CPAR  = (uint32_t)&ADC1->DR;
    DMA1_Channel1->CMAR  = (uint32_t)adc_buf;
    DMA1_Channel1->CNDTR = 4U;
    DMA1_Channel1->CCR   = DMA_CCR_MINC | DMA_CCR_CIRC | DMA_CCR_PSIZE_0 | DMA_CCR_MSIZE_0
                         | DMA_CCR_PL_1 | DMA_CCR_TCIE;
    DMA1_Channel1->CCR  |= DMA_CCR_EN;
    NVIC_SetPriority(DMA1_Channel1_IRQn, 1);
    NVIC_EnableIRQ(DMA1_Channel1_IRQn);

    
    ADC1->CR2 = (ADC1->CR2 & ~(ADC_CR2_EXTSEL | ADC_CR2_EXTTRIG | ADC_CR2_CONT | ADC_CR2_ALIGN))
              | (2UL << ADC_CR2_EXTSEL_Pos) | ADC_CR2_DMA;

   
    TIM1->CR1   = 0U;  TIM1->CR2 = 0U;  TIM1->SMCR = 0U;  TIM1->DIER = 0U;
    TIM1->PSC   = 0U;
    TIM1->ARR   = T_PHASE - 1UL;
    TIM1->RCR   = 0U;
    TIM1->CCMR1 = (6UL << TIM_CCMR1_OC1M_Pos) | TIM_CCMR1_OC1PE
                | (6UL << TIM_CCMR1_OC2M_Pos) | TIM_CCMR1_OC2PE;
    TIM1->CCMR2 = (7UL << TIM_CCMR2_OC3M_Pos) | TIM_CCMR2_OC3PE;
    TIM1->CCER  = TIM_CCER_CC1E | TIM_CCER_CC2E | TIM_CCER_CC3E;
    TIM1->BDTR  = TIM_BDTR_MOE;
    TIM1->CR1   = TIM_CR1_ARPE;

    g_phase = 0U;
    Acq_ProgramPhase(0U);
    TIM1->EGR = TIM_EGR_UG;
    TIM1->SR  = 0U;
    Acq_ProgramPhase(1U);

    NVIC_SetPriority(TIM1_UP_IRQn, 0);
    NVIC_EnableIRQ(TIM1_UP_IRQn);
    TIM1->DIER = TIM_DIER_UIE;
    ADC1->CR2 |= ADC_CR2_EXTTRIG;                                       
    GPIOA->CRH = (GPIOA->CRH & ~0xFFUL) | 0xBBUL;                       
    TIM1->CR1 |= TIM_CR1_CEN;                                           
}

static void Acq_Drain(void)
{
    while (q_tail != q_head)
    {
        Raw_t s = q[q_tail];
        q_tail = (uint16_t)((q_tail + 1U) & (Q_SIZE - 1U));

        if (s.red >= RAW_SATURATION || s.ir >= RAW_SATURATION || s.amb >= RAW_SATURATION) sat_hold = 1600;   /* 2 s */
        else if (sat_hold) sat_hold--;

        Oksimetar_ProcessMultiplexSample(&my_oximeter, s.red, s.ir, s.amb);
    }
}

/* ukljuci / iskljuci merenje (dugme B1) */
static void Measure_Set(uint8_t on)
{
    g_measuring = 0;
    g_leds_on   = 0;
    q_tail = q_head;                                   
    sat_hold = 0;
    Oksimetar_Init(&my_oximeter);                      
    system_active = on;
    if (on) { g_measuring = 1; g_leds_on = 1; }        
}


static void Button_Poll(void)
{
    
    static uint8_t  prev = 1;
    static uint32_t t_last;
    uint8_t lvl = (uint8_t)((GPIOC->IDR >> 13) & 1U);
    if (prev && !lvl)                                  /* padajuca ivica = pritisak */
    {
        uint32_t now = HAL_GetTick();
        if ((now - t_last) > 200U)                     /* zastita od treperenja kontakata */
        {
            t_last = now;
            Measure_Set(!system_active);
        }
    }
    prev = lvl;
}

static uint8_t oled_ok;
static char    oled_status[24] = "Pulsni oksimetar";
static int16_t disp_spo2 = -1, disp_hr = -1;          

static void Oled_Status(const char *s)
{
    uint8_t i = 0;
    while (s[i] && i < sizeof(oled_status) - 1U) { oled_status[i] = s[i]; i++; }
    oled_status[i] = 0;
}

static void Oled_Draw(void)
{
    char b[24];

    SSD1306_Fill(SSD1306_COLOR_BLACK);

    SSD1306_GotoXY(0, 0);
    SSD1306_Puts("Pulsni oksimetar", &Font_7x10, SSD1306_COLOR_WHITE);

    if (disp_spo2 >= 0) sfmt(b, sizeof(b), "SpO2: %d %%", disp_spo2);
    else                sfmt(b, sizeof(b), "SpO2: --");
    SSD1306_GotoXY(0, 24);
    SSD1306_Puts(b, &Font_7x10, SSD1306_COLOR_WHITE);

    if (disp_hr >= 0) sfmt(b, sizeof(b), "HR: %d bpm", disp_hr);
    else              sfmt(b, sizeof(b), "HR: --");
    SSD1306_GotoXY(0, 44);
    SSD1306_Puts(b, &Font_7x10, SSD1306_COLOR_WHITE);

    SSD1306_GotoXY(0, 54);                               /* status: Wi-Fi / server */
    SSD1306_Puts(oled_status, &Font_7x10, SSD1306_COLOR_WHITE);

    SSD1306_UpdateScreen();
}

static void Oled_Refresh(void)
{
    if (oled_ok)
    {
        if (HAL_I2C_IsDeviceReady(&hi2c2, SSD1306_I2C_ADDR, 1, 10) != HAL_OK)
        {
            oled_ok = 0;                                    /* ekran nestao: preskoci, probace opet */
            return;
        }
    }
    else
    {
        if (HAL_I2C_IsDeviceReady(&hi2c2, SSD1306_I2C_ADDR, 2, 30) != HAL_OK) return;   /* nema ga */
        SSD1306_Init();
        oled_ok = 1;
    }
    Oled_Draw();
}


#define ESP_BUF_SZ   512

static char     esp_buf[ESP_BUF_SZ];
static uint16_t esp_n;


static uint8_t  consec_fail;

/* Prijem sa ESP-a ide u DMA kruzni bafer (DMA1 kanal 5 = USART1_RX), pa nijedan bajt ne moze da se
   izgubi dok glavni program racuna SpO2 (jedan obracun traje duze od trajanja jednog bajta na 115200). */
#define RX_RING 512U                                     /* stepen dvojke */
static uint8_t  rx_ring[RX_RING];
static uint16_t rx_tail;

static void Uart_RxStart(void)
{
    RCC->AHBENR |= RCC_AHBENR_DMA1EN;
    DMA1_Channel5->CCR   = 0U;
    DMA1->IFCR           = DMA_IFCR_CGIF5;
    DMA1_Channel5->CPAR  = (uint32_t)&USART1->DR;
    DMA1_Channel5->CMAR  = (uint32_t)rx_ring;
    DMA1_Channel5->CNDTR = RX_RING;
    DMA1_Channel5->CCR   = DMA_CCR_MINC | DMA_CCR_CIRC;  /* 8 bita, periferija -> memorija, bez prekida */
    DMA1_Channel5->CCR  |= DMA_CCR_EN;
    USART1->CR3 |= USART_CR3_DMAR;
    rx_tail = 0U;
}

static void Uart_Config(uint32_t baud)
{
    huart1.Init.BaudRate = baud;
    HAL_UART_Init(&huart1);                              /* brise CR3, pa se DMA prijem ukljucuje iznova */
    Uart_RxStart();
}

static inline int Uart_RxByte(uint8_t *c)
{
    uint16_t head = (uint16_t)(RX_RING - DMA1_Channel5->CNDTR) & (RX_RING - 1U);
    if (head == rx_tail) return 0;
    *c = rx_ring[rx_tail];
    rx_tail = (uint16_t)((rx_tail + 1U) & (RX_RING - 1U));
    return 1;
}

static inline void Uart_Tx(const char *s, uint16_t n)
{
    HAL_UART_Transmit(&huart1, (uint8_t *)s, n, 1000);
}

static inline void Wdg_Kick(void)
{
#if USE_WATCHDOG
    IWDG->KR = 0xAAAA;
#endif
}

static void Wdg_Init(void)
{
#if USE_WATCHDOG
    __HAL_DBGMCU_FREEZE_IWDG();       /* dok si u debageru i stanes na breakpoint, cuvar staje */
    IWDG->KR  = 0x5555;               /* dozvoli upis */
    IWDG->PR  = 6;                    /* LSI ~40 kHz / 256 */
    IWDG->RLR = 0x0FFF;               /* 4096 * 256 / 40 kHz = ~26 s (najmanje ~17 s ako je LSI 60 kHz) */
    IWDG->KR  = 0xAAAA;
    IWDG->KR  = 0xCCCC;               
#endif
}


static void Esp_Clear(void)
{
    { uint8_t d; while (Uart_RxByte(&d)) { } }   
    esp_buf[0] = 0;
    esp_n = 0;
}

static void Esp_Store(uint8_t c)
{
    if (esp_n >= ESP_BUF_SZ - 1)                  
    {
        memmove(esp_buf, esp_buf + ESP_BUF_SZ / 2, ESP_BUF_SZ / 2);
        esp_n = ESP_BUF_SZ / 2 - 1;
    }
    esp_buf[esp_n++] = (char)c;
    esp_buf[esp_n] = 0;
}

static uint8_t Measure_Get(uint8_t *spo2, uint8_t *hr);
static void    Oled_Live(void);

static void Esp_Pump(void)
{
    uint8_t c;
    while (Uart_RxByte(&c)) Esp_Store(c);
    Acq_Drain();
    Button_Poll();                               /* dugme B1 radi i dok se ceka odgovor ESP-a */
}


static uint8_t Esp_Wait2(const char *ok, const char *fail, uint32_t timeout_ms)
{
    uint32_t t0 = HAL_GetTick();
    for (;;)
    {
        Wdg_Kick();
        Esp_Pump();
        if (ok   && strstr(esp_buf, ok))   return 1;
        if (fail && strstr(esp_buf, fail)) return 2;
        if ((HAL_GetTick() - t0) >= timeout_ms) return 0;
    }
}

static uint8_t Esp_Cmd2(const char *cmd, const char *ok, const char *fail, uint32_t timeout_ms)
{
    Esp_Clear();
    Uart_Tx(cmd, (uint16_t)strlen(cmd));
    return Esp_Wait2(ok, fail, timeout_ms);
}

static uint8_t Esp_Cmd(const char *cmd, const char *ok, uint32_t timeout_ms)
{
    return Esp_Cmd2(cmd, ok, "ERROR", timeout_ms) == 1;
}

static void Esp_Idle(uint32_t ms)
{
    uint32_t t0 = HAL_GetTick();
    while ((HAL_GetTick() - t0) < ms) { Wdg_Kick(); Esp_Pump(); Oled_Live(); }   
}

static void Blink(uint8_t times, uint16_t on_ms, uint16_t off_ms)
{
    for (uint8_t i = 0; i < times; i++)
    {
        LD2_ON();  Esp_Idle(on_ms);               
        LD2_OFF(); Esp_Idle(off_ms);
    }
}

static uint8_t Esp_Detect(void)
{
    const uint32_t bauds[2] = { 115200U, 9600U };
    for (uint8_t i = 0; i < 2; i++)
    {
        Uart_Config(bauds[i]);
        for (uint8_t k = 0; k < 3; k++)
        {
            if (Esp_Cmd("AT\r\n", "OK", 1000))
            {
                if (i == 1)                                  /* ESP je na 9600: prebaci ga na 115200 */
                {
                    Esp_Cmd("AT+UART_DEF=115200,8,1,0,0\r\n", "OK", 1000);
                    Uart_Config(115200U);
                    Esp_Idle(100);
                    if (!Esp_Cmd("AT\r\n", "OK", 1000)) return 0;
                }
                return 1;
            }
        }
    }
    return 0;
}

static void Esp_Setup(void)
{
    Esp_Cmd("AT+CWMODE=1\r\n", "OK", 1000);       
    Esp_Cmd("AT+CIPMUX=0\r\n", "OK", 1000);       
}

static uint8_t wifi_err;                       

static uint8_t Wifi_Join(void)
{
    char cmd[128];
    sfmt(cmd, sizeof(cmd), "AT+CWJAP=\"%s\",\"%s\"\r\n", WIFI_NAME, WIFI_PASSWORD);
    if (Esp_Cmd2(cmd, "WIFI GOT IP", "FAIL", 20000) == 1)
    {
        Esp_Wait2("OK", NULL, 1500);
        return 1;
    }
    {
        char *p = strstr(esp_buf, "+CWJAP:");
        wifi_err = p ? (uint8_t)(p[7] - '0') : 9U;
    }
    return 0;
}

static uint8_t Tcp_Open(void)
{
    char     cmd[96];
    uint8_t  r;

    Esp_Cmd("AT+CIPCLOSE\r\n", "OK", 500);        

    sfmt(cmd, sizeof(cmd), "AT+CIPSTART=\"TCP\",\"%s\",%d\r\n", SERVER_IP, SERVER_PORT);
    r = Esp_Cmd2(cmd, "OK", "ERROR", 8000);
    return (r == 1 || strstr(esp_buf, "ALREADY")) ? 1 : 0;
}


static uint8_t Link_Ensure(void)
{
    if (consec_fail >= 3 && !Wifi_Join()) return 0;          /* izgubljen Wi-Fi: ponovo se prijavi */
    return Tcp_Open();
}

static uint8_t Post_Measurement(uint32_t seq, uint8_t spo2, uint8_t hr)
{
    char body[64];
    char req[256];
    char cmd[24];

    int bl = sfmt(body, sizeof(body), "{\"n\":%lu,\"spo2\":%u,\"hr\":%u}",
                      (unsigned long)seq, (unsigned)spo2, (unsigned)hr);
    int rl = sfmt(req, sizeof(req),
                      "POST /api/merenja HTTP/1.1\r\n"
                      "Host: %s:%d\r\n"
                      "Content-Type: application/json\r\n"
                      "Content-Length: %d\r\n"
                      "Connection: close\r\n"
                      "\r\n"
                      "%s",
                      SERVER_IP, SERVER_PORT, bl, body);
    if (bl <= 0 || rl <= 0 || rl >= (int)sizeof(req)) return 0;

    sfmt(cmd, sizeof(cmd), "AT+CIPSEND=%d\r\n", rl);
    if (Esp_Cmd2(cmd, ">", "ERROR", 2000) != 1) return 0;
    if (Esp_Cmd2(req, "SEND OK", "FAIL", 3000) != 1) return 0;
    if (Esp_Wait2("200 OK", "CLOSED", 3000) != 1) return 0;
    Esp_Cmd("AT+CIPCLOSE\r\n", "OK", 1000);                  
    return 1;
}


static uint8_t Measure_Get(uint8_t *spo2, uint8_t *hr)
{
    const Oksimetar *d = &my_oximeter;
    if (!system_active || !d->finger_present || !d->reliable_result || sat_hold) return 0;
    float s = d->spo2 + 0.5f, h = d->heart_rate + 0.5f;
    if (s < 1.0f || h < 1.0f || s > 100.5f || h > 255.0f) return 0;
    *spo2 = (uint8_t)s;
    *hr   = (uint8_t)h;
    return 1;
}


static void Oled_Live(void)
{
    static uint32_t t_last;
    uint32_t now = HAL_GetTick();
    if ((now - t_last) < 400U) return;
    t_last = now;
    uint8_t s, h;
    if (Measure_Get(&s, &h)) { disp_spo2 = s; disp_hr = h; }
    else                     { disp_spo2 = -1; disp_hr = -1; }
    Oled_Refresh();
}

static void Led_Init(void)
{
    GPIO_InitTypeDef g = {0};
    __HAL_RCC_GPIOA_CLK_ENABLE();
    g.Pin   = GPIO_PIN_5;
    g.Mode  = GPIO_MODE_OUTPUT_PP;
    g.Pull  = GPIO_NOPULL;
    g.Speed = GPIO_SPEED_FREQ_LOW;
    HAL_GPIO_Init(GPIOA, &g);
    LD2_OFF();
}


static void Link_Run(void)
{
    Led_Init();
    Acq_Start();                                             /* TIM1 vrti faze; LED ugasene dok se merenje ne ukljuci */
    Measure_Set(AUTO_START);                                 
    Oled_Refresh();                                          
    Esp_Idle(2500);                                          
    Wdg_Init();

    Oled_Status("Trazim ESP...");
    Oled_Refresh();
    while (!Esp_Detect())                                    /* ESP ne odgovara: brzo trepce pa ponovo */
    {
        Wdg_Kick();
        Oled_Status("ESP ne odgovara");
        Oled_Refresh();
        Blink(10, 100, 100);
    }
    Esp_Idle(1000);
    Esp_Setup();

    Oled_Status("Spajam Wi-Fi...");
    Oled_Refresh();
    while (!Wifi_Join())                                     
    {
        char st[24];
        Wdg_Kick();
        sfmt(st, sizeof(st), "Wi-Fi greska %d", (int)wifi_err);
        Oled_Status(st);
        Oled_Refresh();
        Blink(1, 500, 500);
    }
    LD2_ON();
    Oled_Status(system_active ? "Wi-Fi OK" : "Wi-Fi OK  B1=start");

    uint32_t seq   = 1;                                      
    uint32_t next  = HAL_GetTick();
    uint8_t  pend  = 0;                                      
    uint8_t  p_spo2 = 0, p_hr = 0;

    for (;;)
    {
        if (!pend) pend = Measure_Get(&p_spo2, &p_hr);

        if (pend)
        {
            if (Link_Ensure() && Post_Measurement(seq, p_spo2, p_hr))
            {
                char st[24];
                consec_fail = 0;
                LD2_ON();
                sfmt(st, sizeof(st), "Server OK  n=%lu", (unsigned long)seq);
                Oled_Status(st);
                seq++; pend = 0;
            }
            else
            {
                consec_fail++;
                Oled_Status("Greska, ponovo...");
                Blink(3, 150, 150);
                Esp_Cmd("AT+CIPCLOSE\r\n", "OK", 500);       
            }
        }

        next += SEND_PERIOD_MS;
        if ((int32_t)(HAL_GetTick() - next) > 0) next = HAL_GetTick();
        while ((int32_t)(HAL_GetTick() - next) < 0) Esp_Idle(20);
    }
}

/* USER CODE END 0 */

/**
  * @brief  The application entry point.
  * @retval int
  */
int main(void)
{

  /* USER CODE BEGIN 1 */

  /* USER CODE END 1 */

  /* MCU Configuration--------------------------------------------------------*/

  /* Reset of all peripherals, Initializes the Flash interface and the Systick. */
  HAL_Init();

  /* USER CODE BEGIN Init */

  /* USER CODE END Init */

  /* Configure the system clock */
  SystemClock_Config();

  /* USER CODE BEGIN SysInit */

  /* USER CODE END SysInit */

  /* Initialize all configured peripherals */
  MX_GPIO_Init();
  MX_TIM1_Init();
  MX_ADC1_Init();
  MX_I2C2_Init();
  MX_USART1_UART_Init();
  /* USER CODE BEGIN 2 */
  Link_Run();
  /* USER CODE END 2 */

  /* Infinite loop */
  /* USER CODE BEGIN WHILE */
  while (1)
  {
    /* USER CODE END WHILE */

    /* USER CODE BEGIN 3 */

  }
  /* USER CODE END 3 */
}

/**
  * @brief System Clock Configuration
  * @retval None
  */
void SystemClock_Config(void)
{
  RCC_OscInitTypeDef RCC_OscInitStruct = {0};
  RCC_ClkInitTypeDef RCC_ClkInitStruct = {0};
  RCC_PeriphCLKInitTypeDef PeriphClkInit = {0};

  /** Initializes the RCC Oscillators according to the specified parameters
  * in the RCC_OscInitTypeDef structure.
  */
  RCC_OscInitStruct.OscillatorType = RCC_OSCILLATORTYPE_HSI;
  RCC_OscInitStruct.HSIState = RCC_HSI_ON;
  RCC_OscInitStruct.HSICalibrationValue = RCC_HSICALIBRATION_DEFAULT;
  RCC_OscInitStruct.PLL.PLLState = RCC_PLL_ON;
  RCC_OscInitStruct.PLL.PLLSource = RCC_PLLSOURCE_HSI_DIV2;
  RCC_OscInitStruct.PLL.PLLMUL = RCC_PLL_MUL16;
  if (HAL_RCC_OscConfig(&RCC_OscInitStruct) != HAL_OK)
  {
    Error_Handler();
  }

  /** Initializes the CPU, AHB and APB buses clocks
  */
  RCC_ClkInitStruct.ClockType = RCC_CLOCKTYPE_HCLK|RCC_CLOCKTYPE_SYSCLK
                              |RCC_CLOCKTYPE_PCLK1|RCC_CLOCKTYPE_PCLK2;
  RCC_ClkInitStruct.SYSCLKSource = RCC_SYSCLKSOURCE_PLLCLK;
  RCC_ClkInitStruct.AHBCLKDivider = RCC_SYSCLK_DIV1;
  RCC_ClkInitStruct.APB1CLKDivider = RCC_HCLK_DIV2;
  RCC_ClkInitStruct.APB2CLKDivider = RCC_HCLK_DIV1;

  if (HAL_RCC_ClockConfig(&RCC_ClkInitStruct, FLASH_LATENCY_2) != HAL_OK)
  {
    Error_Handler();
  }
  PeriphClkInit.PeriphClockSelection = RCC_PERIPHCLK_ADC;
  PeriphClkInit.AdcClockSelection = RCC_ADCPCLK2_DIV6;
  if (HAL_RCCEx_PeriphCLKConfig(&PeriphClkInit) != HAL_OK)
  {
    Error_Handler();
  }
}

/**
  * @brief ADC1 Initialization Function
  * @param None
  * @retval None
  */
static void MX_ADC1_Init(void)
{

  /* USER CODE BEGIN ADC1_Init 0 */

  /* USER CODE END ADC1_Init 0 */

  ADC_ChannelConfTypeDef sConfig = {0};

  /* USER CODE BEGIN ADC1_Init 1 */

  /* USER CODE END ADC1_Init 1 */

  /** Common config
  */
  hadc1.Instance = ADC1;
  hadc1.Init.ScanConvMode = ADC_SCAN_DISABLE;
  hadc1.Init.ContinuousConvMode = DISABLE;
  hadc1.Init.DiscontinuousConvMode = DISABLE;
  hadc1.Init.ExternalTrigConv = ADC_SOFTWARE_START;
  hadc1.Init.DataAlign = ADC_DATAALIGN_RIGHT;
  hadc1.Init.NbrOfConversion = 1;
  if (HAL_ADC_Init(&hadc1) != HAL_OK)
  {
    Error_Handler();
  }

  /** Configure Regular Channel
  */
  sConfig.Channel = ADC_CHANNEL_0;
  sConfig.Rank = ADC_REGULAR_RANK_1;
  sConfig.SamplingTime = ADC_SAMPLETIME_1CYCLE_5;
  if (HAL_ADC_ConfigChannel(&hadc1, &sConfig) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN ADC1_Init 2 */

  /* USER CODE END ADC1_Init 2 */

}

/**
  * @brief I2C2 Initialization Function
  * @param None
  * @retval None
  */
static void MX_I2C2_Init(void)
{

  /* USER CODE BEGIN I2C2_Init 0 */

  /* USER CODE END I2C2_Init 0 */

  /* USER CODE BEGIN I2C2_Init 1 */

  /* USER CODE END I2C2_Init 1 */
  hi2c2.Instance = I2C2;
  hi2c2.Init.ClockSpeed = 400000;
  hi2c2.Init.DutyCycle = I2C_DUTYCYCLE_2;
  hi2c2.Init.OwnAddress1 = 0;
  hi2c2.Init.AddressingMode = I2C_ADDRESSINGMODE_7BIT;
  hi2c2.Init.DualAddressMode = I2C_DUALADDRESS_DISABLE;
  hi2c2.Init.OwnAddress2 = 0;
  hi2c2.Init.GeneralCallMode = I2C_GENERALCALL_DISABLE;
  hi2c2.Init.NoStretchMode = I2C_NOSTRETCH_DISABLE;
  if (HAL_I2C_Init(&hi2c2) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN I2C2_Init 2 */

  /* USER CODE END I2C2_Init 2 */

}

/**
  * @brief TIM1 Initialization Function
  * @param None
  * @retval None
  */
static void MX_TIM1_Init(void)
{

  /* USER CODE BEGIN TIM1_Init 0 */

  /* USER CODE END TIM1_Init 0 */

  TIM_ClockConfigTypeDef sClockSourceConfig = {0};
  TIM_MasterConfigTypeDef sMasterConfig = {0};
  TIM_OC_InitTypeDef sConfigOC = {0};
  TIM_BreakDeadTimeConfigTypeDef sBreakDeadTimeConfig = {0};

  /* USER CODE BEGIN TIM1_Init 1 */

  /* USER CODE END TIM1_Init 1 */
  htim1.Instance = TIM1;
  htim1.Init.Prescaler = 0;
  htim1.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim1.Init.Period = 19999;
  htim1.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim1.Init.RepetitionCounter = 0;
  htim1.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_ENABLE;
  if (HAL_TIM_Base_Init(&htim1) != HAL_OK)
  {
    Error_Handler();
  }
  sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_INTERNAL;
  if (HAL_TIM_ConfigClockSource(&htim1, &sClockSourceConfig) != HAL_OK)
  {
    Error_Handler();
  }
  if (HAL_TIM_PWM_Init(&htim1) != HAL_OK)
  {
    Error_Handler();
  }
  sMasterConfig.MasterOutputTrigger = TIM_TRGO_UPDATE;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  if (HAL_TIMEx_MasterConfigSynchronization(&htim1, &sMasterConfig) != HAL_OK)
  {
    Error_Handler();
  }
  sConfigOC.OCMode = TIM_OCMODE_PWM1;
  sConfigOC.Pulse = 10000;
  sConfigOC.OCPolarity = TIM_OCPOLARITY_HIGH;
  sConfigOC.OCNPolarity = TIM_OCNPOLARITY_HIGH;
  sConfigOC.OCFastMode = TIM_OCFAST_DISABLE;
  sConfigOC.OCIdleState = TIM_OCIDLESTATE_RESET;
  sConfigOC.OCNIdleState = TIM_OCNIDLESTATE_RESET;
  if (HAL_TIM_PWM_ConfigChannel(&htim1, &sConfigOC, TIM_CHANNEL_1) != HAL_OK)
  {
    Error_Handler();
  }
  if (HAL_TIM_PWM_ConfigChannel(&htim1, &sConfigOC, TIM_CHANNEL_2) != HAL_OK)
  {
    Error_Handler();
  }
  sBreakDeadTimeConfig.OffStateRunMode = TIM_OSSR_DISABLE;
  sBreakDeadTimeConfig.OffStateIDLEMode = TIM_OSSI_DISABLE;
  sBreakDeadTimeConfig.LockLevel = TIM_LOCKLEVEL_OFF;
  sBreakDeadTimeConfig.DeadTime = 0;
  sBreakDeadTimeConfig.BreakState = TIM_BREAK_DISABLE;
  sBreakDeadTimeConfig.BreakPolarity = TIM_BREAKPOLARITY_HIGH;
  sBreakDeadTimeConfig.AutomaticOutput = TIM_AUTOMATICOUTPUT_DISABLE;
  if (HAL_TIMEx_ConfigBreakDeadTime(&htim1, &sBreakDeadTimeConfig) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN TIM1_Init 2 */

  /* USER CODE END TIM1_Init 2 */
  HAL_TIM_MspPostInit(&htim1);

}

/**
  * @brief USART1 Initialization Function
  * @param None
  * @retval None
  */
static void MX_USART1_UART_Init(void)
{

  /* USER CODE BEGIN USART1_Init 0 */

  /* USER CODE END USART1_Init 0 */

  /* USER CODE BEGIN USART1_Init 1 */

  /* USER CODE END USART1_Init 1 */
  huart1.Instance = USART1;
  huart1.Init.BaudRate = 115200;
  huart1.Init.WordLength = UART_WORDLENGTH_8B;
  huart1.Init.StopBits = UART_STOPBITS_1;
  huart1.Init.Parity = UART_PARITY_NONE;
  huart1.Init.Mode = UART_MODE_TX_RX;
  huart1.Init.HwFlowCtl = UART_HWCONTROL_NONE;
  huart1.Init.OverSampling = UART_OVERSAMPLING_16;
  if (HAL_UART_Init(&huart1) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN USART1_Init 2 */

  /* USER CODE END USART1_Init 2 */

}

/**
  * @brief GPIO Initialization Function
  * @param None
  * @retval None
  */
static void MX_GPIO_Init(void)
{
  GPIO_InitTypeDef GPIO_InitStruct = {0};
  /* USER CODE BEGIN MX_GPIO_Init_1 */

  /* USER CODE END MX_GPIO_Init_1 */

  /* GPIO Ports Clock Enable */
  __HAL_RCC_GPIOC_CLK_ENABLE();
  __HAL_RCC_GPIOA_CLK_ENABLE();
  __HAL_RCC_GPIOB_CLK_ENABLE();

  /*Configure GPIO pin : PC13 */
  GPIO_InitStruct.Pin = GPIO_PIN_13;
  GPIO_InitStruct.Mode = GPIO_MODE_IT_RISING;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  HAL_GPIO_Init(GPIOC, &GPIO_InitStruct);

  /* USER CODE BEGIN MX_GPIO_Init_2 */
  {   /* PA8 (crvena LED) i PA9 (IR LED) kao obicni izlazi, pocetno nisko = LED ugasene */
      GPIO_InitTypeDef g = {0};
      HAL_GPIO_WritePin(GPIOA, LED_RED_Pin | LED_IR_Pin, GPIO_PIN_RESET);
      g.Pin   = LED_RED_Pin | LED_IR_Pin;
      g.Mode  = GPIO_MODE_OUTPUT_PP;
      g.Pull  = GPIO_NOPULL;
      g.Speed = GPIO_SPEED_FREQ_HIGH;
      HAL_GPIO_Init(GPIOA, &g);
  }

  /* USER CODE END MX_GPIO_Init_2 */
}

/* USER CODE BEGIN 4 */

/* USER CODE END 4 */

/**
  * @brief  This function is executed in case of error occurrence.
  * @retval None
  */
void Error_Handler(void)
{
  /* USER CODE BEGIN Error_Handler_Debug */
  /* User can add his own implementation to report the HAL error return state */
  __disable_irq();
  while (1)
  {
  }
  /* USER CODE END Error_Handler_Debug */
}
#ifdef USE_FULL_ASSERT
/**
  * @brief  Reports the name of the source file and the source line number
  *         where the assert_param error has occurred.
  * @param  file: pointer to the source file name
  * @param  line: assert_param error line source number
  * @retval None
  */
void assert_failed(uint8_t *file, uint32_t line)
{
  /* USER CODE BEGIN 6 */
  /* User can add his own implementation to report the file name and line number,
     ex: printf("Wrong parameters value: file %s on line %d\r\n", file, line) */
  /* USER CODE END 6 */
}
#endif /* USE_FULL_ASSERT */
