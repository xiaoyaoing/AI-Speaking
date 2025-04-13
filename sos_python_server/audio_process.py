import io
import os
import random

import librosa
import librosa.feature
import matplotlib.pyplot as plt
import numpy as np
import pesq
import pydub
import soundfile as sf
from pydub import AudioSegment

dir = r'.'


# pydub.utils.FFMPEG_PATH = r"C:\Users\Yalin Feng\Downloads\Compressed\ffmpeg-7.0.1\ffmpeg-7.0.1-essentials_build\bin\ffmpeg.exe"
def convert_float32_to_mp3(raw_bytes, sample_rate=44100):
    # 将字节转换为浮点数数组
    audio_array = np.frombuffer(raw_bytes, dtype=np.float32)

    # 转换浮点数组到整数，因为AudioSegment需要整数数据
    int_array = np.int16(audio_array * 32767)

    # 使用pydub创建音频段，sample_width=2 表示16位音频
    audio_segment = AudioSegment(
        data=int_array.tobytes(),
        sample_width=2,
        frame_rate=sample_rate,
        channels=1
    )

    # 将音频段导出为MP3
    mp3_buffer = io.BytesIO()
    audio_segment.export(mp3_buffer, format="mp3")
    mp3_byte = mp3_buffer.getvalue()
    with open("received_audio.mp3", "wb") as file:
        file.write(mp3_byte)
    return mp3_byte  # 返回MP3文件的字节


# TODO: 声音接口
def audio_process(audio):
    """
    输入一段音频，格式是mp3(格式其实无所谓，可以现转)，对这段音频进行分析评估，返回一个字典，字典里装对这段声音的
    评分、评价之类的。
    :param audio: 待评价的音频
    :return: 一组反馈
    """
    try:
        # 读取音频文件
        y, sr = librosa.load(audio)

        # 提取特征
        # 例如，可以使用音频的节奏（tempo）、语调（pitch）、声音能量（energy）等特征
        tempo, _ = librosa.beat.beat_track(y=y, sr=sr)
        pitch, _ = librosa.core.piptrack(y=y, sr=sr)
        energy = np.mean(librosa.feature.rms(y=y))

        # 计算综合评分
        # 这里可以根据需要定义评分规则在 '__init__.pyi' 中找不到引用 'feature'
        # 例如，可以使用加权平均来计算综合评分
        pitch = pitch.mean()
        score = tempo * 0.4 + pitch * 0.3 + energy * 300
    except Exception as e:
        print("Error analyzing audio:", e)
        return None
    feedback = ''
    if score >= 90:
        feedback = "Excellent"
    elif score > 60:
        feedback = "Good"
    else:
        feedback = "Not Enough"
    path = dir + os.sep + "newVOA.wav"
    clean_signal, fs = sf.read(path)
    noisy_signal, fs = sf.read(audio)

    clean_signal = clean_signal[:, 0]
    noisy_signal = noisy_signal[:, 0]
    pesq_score = pesq.pesq(16000, clean_signal, noisy_signal, 'wb')
    print(pesq_score)
    plot_radar_chart([float(tempo[0] * 0.5), pitch * 1.5, energy * 1000, (float(pesq_score) + 0.5) * 20, float(score)],
                     ["tempo", "pitch", "energy", "pesq", "score"])
    return {"message": "Audio received and processed successfully!",
            "feedback": random.choice(
                ["good speaking, ", "excellent, ", "keep this! "]) +
                        f"\nscore: {str(round(float(score), 2))};"
            }


def plot_radar_chart(data, labels):
    # Number of variables
    num_vars = len(data)

    # Compute angle of each axis
    angles = np.linspace(0, 2 * np.pi, num_vars, endpoint=False).tolist()

    # Make the plot close to a circle
    data += data[:1]
    angles += angles[:1]

    # Plotting
    fig, ax = plt.subplots(figsize=(6, 6), subplot_kw=dict(polar=True))

    # Draw one axe per variable and add labels
    ax.set_theta_offset(np.pi / 2)
    ax.set_theta_direction(-1)
    plt.xticks(angles[:-1], labels)
    # Plot data
    print(angles)
    print(data)
    ax.plot(angles, data, linewidth=1, linestyle='solid')
    plt.show()
    return fig


# 示例用法
# 假设 raw_bytes 是从Unity接收到的原始音频字节数据
# mp3_bytes = convert_float32_to_mp3(raw_bytes)
# with open("output.mp3", "wb") as f:
#     f.write(mp3_bytes)

if __name__ == '__main__':
    # print(pydub.__file__)
    fb = audio_process("./received_audio.mp3")
    print(fb)
