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
    print(f"Sentiment: {result}\n")

