import pickle
import json
import argparse

def extract_params_from_pkl(pkl_path, json_path):
    # pkl 파일에서 데이터 로드 시 encoding='latin1' 사용
    with open(pkl_path, 'rb') as f:
        data = pickle.load(f, encoding='latin1')  # Python 2에서 생성된 pickle 파일 호환

    # 필요한 'pose'와 'betas' 변수 추출
    pose = data['pose'].tolist()  # numpy array를 list로 변환
    betas = data['betas'].tolist()  # numpy array를 list로 변환

    # 추출된 데이터를 딕셔너리 형태로 저장
    params = {
        'pose': pose,
        'betas': betas
    }

    # JSON 파일로 저장
    with open(json_path, 'w') as f:
        json.dump(params, f, indent=4)

    print(f"Parameters saved to {json_path}")

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description="Extract 'pose' and 'betas' from pkl and save as JSON")
    parser.add_argument('pkl_path', type=str, help="Path to the input .pkl file")
    parser.add_argument('json_path', type=str, help="Path to the output .json file")

    args = parser.parse_args()

    extract_params_from_pkl(args.pkl_path, args.json_path)
