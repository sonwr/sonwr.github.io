# Sentiment Analysis using Hugging Face Transformers

## 개요

이 프로젝트는 Hugging Face의 `transformers`와 IMDB 영화 리뷰 데이터셋을 사용하여 감정 분석을 수행합니다. 사전 학습된 DistilBERT 모델을 미세 조정(fine-tuning)하여 감정 분석의 정확도를 높이는 과정을 다룹니다.

## 요구 사항

- Python 3.9.5
- Hugging Face Transformers
- Datasets
- Evaluate
- PyTorch

## 설치

필요한 패키지를 설치하려면 다음 명령어를 사용하세요:

```bash
pip install transformers datasets evaluate torch
```

## 코드 설명

### 간단한 감정 분석

사전 학습된 모델을 사용하여 간단한 감정 분석을 수행합니다.

```python
from transformers import pipeline

# 감정 분석 파이프라인 로드
sentiment_pipeline = pipeline("sentiment-analysis")

# 분석할 텍스트 정의
texts = [
    "I love this product! It is absolutely amazing.",
    "This is the worst experience I've ever had.",
    "The movie was okay, but not great.",
    "I am so happy with the service.",
    "I am disappointed with the quality of the item."
]

# 감정 분석 수행
results = sentiment_pipeline(texts)

# 결과 출력
for text, result in zip(texts, results):
    print(f"Text: {text}")
    print(f"Sentiment: {result}
")
```

위 코드는 `sentiment_analysis.py`로 저장되어 있습니다.

### 모델 미세 조정을 통한 감정 분석

IMDB 데이터셋을 사용하여 사전 학습된 DistilBERT 모델을 미세 조정하고, 감정 분석을 수행합니다.

```python
import torch
from transformers import DistilBertTokenizerFast, DistilBertForSequenceClassification, Trainer, TrainingArguments
from datasets import load_dataset
import evaluate
import numpy as np

# 데이터셋 로드
dataset = load_dataset('imdb')

# 데이터셋을 학습 및 평가 세트로 분할
train_dataset = dataset['train'].shuffle(seed=42).select(range(2000))
test_dataset = dataset['test'].shuffle(seed=42).select(range(500))

# 토크나이저 및 모델 로드
tokenizer = DistilBertTokenizerFast.from_pretrained('distilbert-base-uncased')
model = DistilBertForSequenceClassification.from_pretrained('distilbert-base-uncased')

# 데이터셋을 토큰화
def tokenize_function(examples):
    return tokenizer(examples['text'], padding="max_length", truncation=True)

train_dataset = train_dataset.map(tokenize_function, batched=True)
test_dataset = test_dataset.map(tokenize_function, batched=True)

train_dataset = train_dataset.remove_columns(['text'])
test_dataset = test_dataset.remove_columns(['text'])

train_dataset.set_format('torch')
test_dataset.set_format('torch')

# 평가 메트릭 정의
metric = evaluate.load('accuracy')

def compute_metrics(eval_pred):
    logits, labels = eval_pred
    predictions = torch.argmax(torch.tensor(logits), dim=-1)
    return metric.compute(predictions=predictions, references=labels)

# 학습 인자 정의
training_args = TrainingArguments(
    output_dir='./results',
    eval_strategy="epoch",
    learning_rate=2e-5,
    per_device_train_batch_size=8,
    per_device_eval_batch_size=8,
    num_train_epochs=3,
    weight_decay=0.01,
)

# Trainer 정의
trainer = Trainer(
    model=model,
    args=training_args,
    train_dataset=train_dataset,
    eval_dataset=test_dataset,
    compute_metrics=compute_metrics,
)

# 모델 학습
trainer.train()

# 학습된 모델로 감정 분석 파이프라인 생성
from transformers import pipeline

sentiment_pipeline = pipeline("sentiment-analysis", model=model, tokenizer=tokenizer)

# 분석할 텍스트 정의
texts = [
    "I love this product! It is absolutely amazing.",
    "This is the worst experience I've ever had.",
    "The movie was okay, but not great.",
    "I am so happy with the service.",
    "I am disappointed with the quality of the item."
]

# 감정 분석 수행
results = sentiment_pipeline(texts)

# 결과 출력
for text, result in zip(texts, results):
    print(f"Text: {text}")
    print(f"Sentiment: {result}
")
```

위 코드는 `sentiment_analysis_imdb.py`로 저장되어 있습니다.

## 기대 효과

- **실생활 문제 해결**: 고객 피드백 분석, 소셜 미디어 모니터링, 제품 리뷰 분석 등 다양한 실생활 문제에 적용 가능.
- **NLP 기술 이해**: 최신 NLP 기술과 모델 활용법 학습.
- **모델 성능 향상**: 사전 학습된 모델을 특정 도메인에 맞게 미세 조정하여 성능 최적화.

## 제한점

- **데이터 의존성**: 모델 성능은 학습 데이터셋의 품질과 다양성에 크게 의존.
- **연산 자원 요구**: 대규모 모델 학습 및 평가에 많은 연산 자원과 시간 필요.
- **모델 편향성**: 학습 데이터의 편향성이 모델에 반영될 가능성.
- **한계적인 이해 능력**: 복잡한 맥락적 의미나 감정 상태 이해에 한계.

## 참고 자료

- Hugging Face Transformers: https://huggingface.co/docs/transformers/index
- Datasets: https://huggingface.co/docs/datasets/
- Evaluate: https://huggingface.co/docs/evaluate/index
- PyTorch: https://pytorch.org/docs/stable/index.html
- IMDB Dataset: https://huggingface.co/datasets/imdb
- Hugging Face Model Hub: https://huggingface.co/models
- Pipeline API: https://huggingface.co/docs/transformers/main_classes/pipelines
- Hugging Face Tutorials: https://huggingface.co/learn/nlp-course/chapter1
