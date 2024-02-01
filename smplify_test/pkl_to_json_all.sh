#!/bin/bash

# 시작 번호와 끝 번호 설정
start=1800
end=3600

# 지정된 범위 내에서 반복
for i in $(seq $start $end); do
    # 입력 파일과 출력 파일 이름 설정
    input_file="f_${i}_4_output_smplify.pkl"
    output_file="f_${i}_4_output_smplify.json"

    # 스크립트 실행
    echo "Converting $input_file to $output_file..."
    python pkl_to_json.py "$input_file" "$output_file"
done

echo "Conversion completed."
