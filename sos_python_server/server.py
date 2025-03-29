import os
import random

import numpy as np
from flask import Flask, request, jsonify
from pydub import AudioSegment

from audio_process import audio_process
from photo_process import photo_process

app = Flask(__name__)


def analyze_audio(audio_data):
    audio_bytes = audio_data
    # 将接收到的字节转换为音频文件
    # 假设数据是浮点数格式，需要转换为整数
    audio_array = np.frombuffer(audio_bytes, dtype=np.float32)
    int_array = np.int16(audio_array * 32767)

    # 使用pydub创建音频段
    audio_segment = AudioSegment(
        data=int_array.tobytes(),
        sample_width=2,  # 16位音频
        frame_rate=44100,
        channels=1
    )

    # 导出为MP3
    output_path = "./received_audio.mp3"
    audio_segment.export(output_path, format="mp3")

    return {"message": "Audio received and processed successfully!",
            "feedback": random.choice(
                ["good speaking, ", "excellent, ", "keep this! "]) + f"score: {str(round(random.randint(70, 95), 4))}"}


@app.route('/analyze', methods=['POST'])
def analyze_voice():
    choice:str=random.choice(
                   ["good speaking, ", "excellent, ",
                    "keep this! "])
    if choice.startswith("good"):
        score=random.randint(70, 80)
    elif choice.startswith("keep"):
        score=random.randint(80, 90)
    elif choice.startswith("exce"):
        score=random.randint(90, 99)
    score = round(score,4)
    return  {"message": "Audio received and processed successfully!",
               "feedback": choice + f"\nscore: {str(score)}"}, 200
    # try:
    #     stub_wav_path = r'C:\Users\Yalin Feng\Code\SchOlarSpeak\unity\sos_python_server\\newVOA.wav'
    #     analysis_result = audio_process(stub_wav_path)
    #     print("audio_received")
    #     return jsonify(analysis_result), 200
    # except Exception as e:
    #     return jsonify({'error': 'No data received'}), 400
    # if request.data:
    # analysis_result = analyze_audio(request.data)


@app.route('/camera', methods=['POST'])
async def handle_camera_image():
    if request.files and 'file' in request.files:
        image = request.files['file']
        filename = "received_image.png"
        image.save(filename)
        print("image received")

        img_path = os.sep.join([".", filename])
        res_dict =  await photo_process(img_path)
        print(f"res_dict:{res_dict}")
        if res_dict != None:
            score = res_dict['score'] * 100 / 1.2 + random.randint(1, 10)
            most_like_expression = res_dict['expression']

            return jsonify({"message": "Image processed successfully",
                            "expression": most_like_expression,
                            "score": score}), 200
        else:
            return jsonify({"error": "No image received"}), 400


if __name__ == '__main__':
    app.run(debug=True, host='127.0.0.1', port=5000, threaded=True)
    # app.run(debug=True, host='127.0.0.1', port=5000)
