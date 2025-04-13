import os
from openai import OpenAI


api_key="YOUR_KEY_FROM_DEEPSEEK"
client = OpenAI(api_key=api_key, base_url="https://api.deepseek.com")

max_tokens = 32000
warning_threshold = 0.9


def create_response(messages, output_file_path=None) -> str:
    response = client.chat.completions.create(
        model="deepseek-chat",
        messages=messages,
        max_tokens=4096,
        temperature=0,
        stream=False,
        n=1
    )

    # 检查响应
    if response.choices:
        response_content = response.choices[0].message.content
        print(f"Prompt token usage: {response.usage}")

        # 如果提供了写入文件，则准备输出文件的路径并写入响应结果
        if output_file_path:
            with open(output_file_path, 'w', encoding='utf-8') as output_file:
                output_file.write(response_content)
        if response.usage.total_tokens < max_tokens * warning_threshold:
            return "200"
        else:
            return "TOKEN_WARNING"
    else:
        print(f"Failed to get a response for file: {output_file_path}")
        return str(response)

def feedback(messages, voice_path):
    output_file_path = os.path.join(voice_path)
    return create_response(messages, output_file_path)

    
    
def prompt(voice_data: str,voice_content_path:str):
    # 编写 prompt
    sys_prompt_result = f'''\
    # Task
    ## Instructions:
    You are an AI assistant designed to evaluate and provide feedback on academic presentations within a simulation system. The user of the system, acting as a presenter, will input their speech under the section titled "What speaker says." Your task is to assess the quality of the presentation based on several criteria, including clarity, organization, content accuracy, and engagement.

    ## Evaluation Criteria:

    Clarity;
    Organization;
    Content Accuracy;
    Engagement;

    ## Feedback Guidelines:
    Be specific.
    Suggest improvements where necessary.
    Be brief.
    '''

    # 创建prompt的输入（消息列表
    messages = [
        {"role": "system", "content": sys_prompt_result},
        {"role": "user", "content":  voice_data}
    ]
    
        # messages.append({"role": "user", "content": PHASE3 + "\n" + data["source_sink"] + "\n" + data["calls"]})
    res = feedback(messages, voice_content_path)
    if res != "200":
        print("[Error] summary" + res)
        print("voice content:"+voice_content_path)
    return res