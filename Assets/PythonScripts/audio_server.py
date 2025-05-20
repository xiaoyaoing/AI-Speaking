#!/usr/bin/env python3

import asyncio
import websockets
import json
import os
import time
import argparse

# 从文件读取音频数据
def load_audio_file(file_path):
    try:
        with open(file_path, 'rb') as f:
            return f.read()
    except Exception as e:
        print(f"加载音频文件出错: {e}")
        return None

# WebSocket服务器处理程序
async def handle_connection(websocket, path):
    print(f"客户端连接: {websocket.remote_address}")
    
    try:
        # 发送欢迎消息
        await websocket.send("type:status|message:Connected to Python audio server")
        
        # 处理接收到的消息
        async for message in websocket:
            try:
                print(f"收到消息: {message}")
                
                # 处理命令
                if message.startswith("cmd:"):
                    cmd = message[4:]
                    
                    # 发送音频命令
                    if cmd.startswith("send_audio:"):
                        file_path = cmd[11:]
                        print(f"请求发送音频文件: {file_path}")
                        
                        # 读取音频文件
                        audio_data = load_audio_file(file_path)
                        if audio_data:
                            # 先发送问题文本信息
                            await websocket.send(f"type:question_info|text:这是来自Python的音频: {os.path.basename(file_path)}")
                            # 然后发送二进制音频数据
                            await websocket.send(audio_data)
                            print(f"已发送音频文件: {file_path} ({len(audio_data)} 字节)")
                        else:
                            await websocket.send(f"type:error|message:无法加载音频文件: {file_path}")
                    
                    # 设置播放速度命令
                    elif cmd.startswith("set_speed:"):
                        try:
                            speed = float(cmd[10:])
                            await websocket.send(f"type:playback_control|speed:{speed}")
                            print(f"已设置播放速度: {speed}")
                        except ValueError:
                            await websocket.send("type:error|message:无效的播放速度值")
                    
                    # 未知命令
                    else:
                        await websocket.send(f"type:error|message:未知命令: {cmd}")
                        
            except Exception as e:
                print(f"处理消息时出错: {e}")
                await websocket.send(f"type:error|message:处理消息时出错: {e}")
                
    except websockets.exceptions.ConnectionClosed:
        print("连接已关闭")
    except Exception as e:
        print(f"处理连接时出错: {e}")

# 主函数
async def main(host, port):
    server = await websockets.serve(handle_connection, host, port)
    print(f"音频服务器已启动: ws://{host}:{port}")
    
    # 保持服务器运行
    await server.wait_closed()

# 程序入口
if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="WebSocket音频服务器")
    parser.add_argument("--host", default="localhost", help="服务器主机名")
    parser.add_argument("--port", type=int, default=8765, help="服务器端口")
    args = parser.parse_args()
    
    # 启动服务器
    asyncio.run(main(args.host, args.port))

"""
使用示例:

1. 安装所需的Python库:
   pip install websockets

2. 启动服务器:
   python audio_server.py --host localhost --port 8765

3. 准备测试用的WAV格式音频文件，放在已知路径。

4. 在Unity中连接到服务器(自动或手动)。

5. 使用命令发送音频:
   - 在Unity中通过WebSocketManager发送消息: "cmd:send_audio:/path/to/audio.wav"
   - 或者直接在Python控制台发送音频，实现了客户端推送

注意:
- 音频文件必须是WAV格式
- 路径使用系统本地路径格式
""" 