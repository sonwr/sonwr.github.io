#!/bin/bash

# Data 폴더의 이름을 변수로 설정
DATA_FOLDER="Set1_V6"

# f_1800부터 f_3600까지의 프레임 인덱스를 반복합니다.
#for i in $(seq 1800 3600)
#for i in $(seq 1800 1800)
for i in $(seq 2100 2350)
do
    # 파일 복사 및 이름 변경
    cp -f "${DATA_FOLDER}/f_${i}_0_resize.jpg" "input.jpg"
    cp -f "${DATA_FOLDER}/f_${i}_1_joint_pos.json" "input_joints.json"
    cp -f "${DATA_FOLDER}/f_${i}_2_confid.json" "input_conf.json"

    # Python 스크립트 실행
    python fit_3d_robot.py --out_dir ./

    # 생성된 파일들을 Data 폴더로 이동 및 이름 변경
    mv -f "output_jtr.json" "${DATA_FOLDER}/f_${i}_3_joint_3d_smplify1.json"
    mv -f "output_jtr2.json" "${DATA_FOLDER}/f_${i}_3_joint_3d_smplify2.json"
    mv -f "output_jtr3.json" "${DATA_FOLDER}/f_${i}_3_joint_3d_smplify3.json"
    mv -f "output_jtr4.json" "${DATA_FOLDER}/f_${i}_3_joint_3d_smplify4.json"
    mv -f "output.png" "${DATA_FOLDER}/f_${i}_4_output_smplify.png"
    mv -f "output.pkl" "${DATA_FOLDER}/f_${i}_4_output_smplify.pkl"
    mv -f "output.json" "${DATA_FOLDER}/f_${i}_4_output_smplify.json"

    # 현재 디렉토리의 임시 파일들을 정리
    rm input.jpg input_joints.json input_conf.json
done

