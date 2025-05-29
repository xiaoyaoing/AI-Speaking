# -*- coding: utf-8 -*-
import os
import sys
import comtypes.client
import time
from PIL import Image
import traceback

# 设置标准输出编码为UTF-8
if sys.stdout.encoding != 'utf-8':
    sys.stdout.reconfigure(encoding='utf-8')
if sys.stderr.encoding != 'utf-8':
    sys.stderr.reconfigure(encoding='utf-8')

def convert_pptx_to_images(pptx_path, output_folder, image_format='png'):
    """
    将PPTX文件转换为图片序列
    
    参数:
        pptx_path: PPTX文件路径
        output_folder: 输出文件夹路径
        image_format: 输出图片格式 (默认: png)
    """
    powerpoint = None
    presentation = None
    
    # 标准化路径，确保使用正确的路径分隔符
    output_folder = os.path.normpath(output_folder)
    pptx_path = os.path.normpath(pptx_path)
    
    print(f"标准化后的输出路径: {output_folder}")
    
    # 确保输出文件夹存在并检查权限
    try:
        if not os.path.exists(output_folder):
            os.makedirs(output_folder)
            print(f"创建输出目录: {output_folder}")
        
        # 测试写入权限
        test_file = os.path.join(output_folder, "test_write.tmp")
        with open(test_file, 'w') as f:
            f.write("test")
        os.remove(test_file)
        print("输出目录写入权限检查通过")
        
    except Exception as folder_error:
        print(f"输出目录创建或权限检查失败: {str(folder_error)}")
        return False
    
    # 确保输出文件夹存在
    if not os.path.exists(output_folder):
        os.makedirs(output_folder)
    
    try:
        # 初始化PowerPoint应用程序
        print("正在启动PowerPoint应用程序...")
        powerpoint = comtypes.client.CreateObject("PowerPoint.Application")
        
        # 注意：某些PowerPoint版本不允许设置Visible=False
        # 如果需要隐藏窗口，可以尝试最小化
        try:
            powerpoint.Visible = True  # 保持可见，避免COM错误
            print("PowerPoint应用程序已启动（可见模式）")
        except Exception as visible_error:
            print(f"设置PowerPoint可见性时出现警告: {str(visible_error)}")
            # 继续执行，不影响转换过程
        
        # 获取当前PowerPoint版本
        version = powerpoint.Version
        print(f"PowerPoint版本: {version}")
        
        # 打开演示文稿
        print(f"正在打开文件: {pptx_path}")
        presentation = powerpoint.Presentations.Open(os.path.abspath(pptx_path))
        
        # 获取幻灯片数量
        slide_count = presentation.Slides.Count
        print(f"开始转换 {slide_count} 张幻灯片...")
        
        success_count = 0
        error_count = 0
        
        # 遍历每一张幻灯片
        for i in range(1, slide_count + 1):
            try:
                print(f"正在处理第 {i} 张幻灯片...")
                
                # 导出当前幻灯片为图片
                slide = presentation.Slides(i)
                
                # 使用更简单的文件名，避免路径问题
                temp_filename = f"temp_{i:03d}.{image_format}"
                final_filename = f"slide_{i:03d}.{image_format}"
                temp_image_path = os.path.join(output_folder, temp_filename)
                final_image_path = os.path.join(output_folder, final_filename)
                
                print(f"尝试导出到: {temp_image_path}")
                
                # 导出为图片 - 使用更安全的方法
                export_success = False
                
                # 方法1：标准导出
                try:
                    slide.Export(temp_image_path, image_format.upper())
                    export_success = True
                    print(f"标准方法成功导出第 {i} 张幻灯片")
                except Exception as export_error:
                    print(f"标准导出方法失败: {str(export_error)}")
                
                # 方法2：指定尺寸导出
                if not export_success:
                    try:
                        slide.Export(temp_image_path, "PNG", 1920, 1080)
                        export_success = True
                        print(f"指定尺寸方法成功导出第 {i} 张幻灯片")
                    except Exception as backup_error:
                        print(f"指定尺寸导出方法失败: {str(backup_error)}")
                
                # 方法3：使用默认尺寸PNG导出
                if not export_success:
                    try:
                        slide.Export(temp_image_path, "PNG")
                        export_success = True
                        print(f"默认PNG方法成功导出第 {i} 张幻灯片")
                    except Exception as png_error:
                        print(f"默认PNG导出方法失败: {str(png_error)}")
                
                # 如果所有导出方法都失败
                if not export_success:
                    print(f"所有导出方法都失败，跳过第 {i} 张幻灯片")
                    error_count += 1
                    continue
                
                # 检查临时文件是否创建成功
                if not os.path.exists(temp_image_path):
                    print(f"第 {i} 张幻灯片导出失败：未生成临时文件")
                    error_count += 1
                    continue
                
                # 使用PIL旋转图片
                try:
                    with Image.open(temp_image_path) as img:
                        # 顺时针旋转90度
                        rotated_img = img.rotate(-90, expand=True)
                        # 保存旋转后的图片
                        rotated_img.save(final_image_path)
                    
                    # 删除临时文件
                    os.remove(temp_image_path)
                    success_count += 1
                    print(f"成功转换第 {i}/{slide_count} 张幻灯片")
                    
                except Exception as image_error:
                    print(f"处理第 {i} 张幻灯片图像时出错: {str(image_error)}")
                    error_count += 1
                    # 清理临时文件
                    if os.path.exists(temp_image_path):
                        os.remove(temp_image_path)
                    continue
                
                # 添加短暂延迟，避免系统负载过高
                time.sleep(0.1)
                
            except Exception as slide_error:
                print(f"处理第 {i} 张幻灯片时出现未知错误: {str(slide_error)}")
                print(f"错误类型: {type(slide_error).__name__}")
                error_count += 1
                continue
        
        print(f"\n转换统计:")
        print(f"成功转换: {success_count} 张")
        print(f"转换失败: {error_count} 张")
        print(f"总计: {slide_count} 张")
        print(f"图片已保存到: {output_folder}")
        
        return success_count > 0  # 只要有一张成功就算成功
        
    except Exception as e:
        print(f"转换过程中出现严重错误: {str(e)}")
        print(f"错误类型: {type(e).__name__}")
        print("详细错误信息:")
        traceback.print_exc()
        return False
    
    finally:
        # 安全关闭资源
        try:
            if presentation:
                print("正在关闭演示文稿...")
                presentation.Close()
        except Exception as close_error:
            print(f"关闭演示文稿时出错: {str(close_error)}")
        
        try:
            if powerpoint:
                print("正在退出PowerPoint...")
                powerpoint.Quit()
        except Exception as quit_error:
            print(f"退出PowerPoint时出错: {str(quit_error)}")

def main():
    if len(sys.argv) != 3:
        print("用法: python ppt_to_images.py <pptx_path> <output_folder>")
        sys.exit(1)
    
    pptx_path = sys.argv[1]
    output_folder = sys.argv[2]
    
    # 检查文件是否存在
    if not os.path.exists(pptx_path):
        print(f"错误：找不到指定的PPT文件！路径: {pptx_path}")
        sys.exit(1)
    
    # 检查文件扩展名
    if not pptx_path.lower().endswith(('.ppt', '.pptx')):
        print("错误：请提供有效的PPT文件（.ppt或.pptx）！")
        sys.exit(1)
    
    # 开始转换
    print("\n开始转换PPT为图片...")
    print(f"输入文件: {pptx_path}")
    print(f"输出目录: {output_folder}")
    
    # 确保PowerPoint没有在运行
    print("正在检查并关闭现有的PowerPoint进程...")
    os.system('taskkill /f /im POWERPNT.EXE 2>nul')
    time.sleep(2)
    
    success = convert_pptx_to_images(pptx_path, output_folder)
    
    if success:
        print("\n转换完成！")
        sys.exit(0)
    else:
        print("\n转换失败！")
        sys.exit(1)

if __name__ == "__main__":
    main() 