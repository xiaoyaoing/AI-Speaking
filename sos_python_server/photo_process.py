import os.path
import aiohttp
import asyncio
import requests
import json
from PIL import Image
import io
import base64

eyes_moving = True


async def photo_process(img_path: str) -> dict:
    """
    传入一个png格式的图片，分析图片中的眼动、注意力不集中之类的问题，返回一个字典作为反馈。
    :param image: 待分析图片
    :return: 反馈
    """
    # if os.path.exists(img_path):
    #     print(f"{img_path} exists")
    # else:
    #     print(f"{img_path} does not exist")
    #     return None
    img_path = "./received_image.png"
    try:
        API_URL = 'https://ms-fc-fapp-func-cpihvblfpx.cn-shanghai.fcapp.run/invoke'

        def post_request(url, json):
            with requests.Session() as session:
                response = session.post(url, json=json, )
                return response

        with Image.open(img_path) as img:
            # 将图片转换为字节流
            img_bytes = io.BytesIO()
            img.save(img_bytes, format='PNG')  # 保存图片为PNG格式到字节流中
            img_bytes = img_bytes.getvalue()  # 获取字节流的内容
            img_base64 = base64.b64encode(img_bytes).decode('utf-8')

            payload = {"input": {"image": img_base64}}

            response = post_request(API_URL, json=payload)

            response_dict = json.loads(response.content)
            most_like_expression = response_dict['Data']['labels'][0]
            labels = response_dict['Data']['labels']
            possibilities = response_dict['Data']['scores']
            rating = {
                "Angry": 0.4,  # 愤怒
                "Disgust": 0.3,  # 恶心
                "Fear": 0.5,  # 害怕
                "Happy": 1,  # 开心
                "Sad": 0.6,  # 悲伤
                "Surprise": 0.8,  # 惊喜
                "Neutral": 1,  # 平和
            }
            score: float = 0.0
            total_rate: float = 0.0

            for i, label in enumerate(labels):
                prate = possibilities[i]
                score += prate * 1 * rating[label]
                total_rate += prate

            score /= total_rate

            return {
                "score": round(score, 4),
                "expression": most_like_expression,
                "eyes moving": eyes_moving
            }
    except Exception as e:
        return None


if __name__ == '__main__':
    async def get_res():
        res =await photo_process("")
        return res


    result =  asyncio.run(get_res())
    print(result)
