from pydub import AudioSegment
import numpy as np
import io


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

    return mp3_buffer.getvalue()  # 返回MP3文件的字节


# TODO: 声音接口
def audio_process(audio):
    """
    输入一段音频，格式是mp3(格式其实无所谓，可以现转)，对这段音频进行分析评估，返回一个字典，字典里装对这段声音的
    评分、评价之类的。
    :param audio: 待评价的音频
    :return: 一组反馈
    """
    feedback = "good"
    credit = 100
    return {
        "feedback": feedback,
        "credit": credit,
        "others": 123123
    }

# 示例用法
# 假设 raw_bytes 是从Unity接收到的原始音频字节数据
# mp3_bytes = convert_float32_to_mp3(raw_bytes)
# with open("output.mp3", "wb") as f:
#     f.write(mp3_bytes)
