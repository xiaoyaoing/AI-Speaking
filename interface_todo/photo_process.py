# TODO: 图像接口
def photo_process(image):
    """
    传入一个png格式的图片，分析图片中的眼动、注意力不集中之类的问题，返回一个字典作为反馈。
    :param image: 待分析图片
    :return: 反馈
    """
    return {
        "eyes moving": True,
        "focus point": 80,
        "others": 123123
    }