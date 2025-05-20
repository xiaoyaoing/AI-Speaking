import os
import sys
import comtypes.client
import time
from PIL import Image

def convert_pptx_to_images(pptx_path, output_folder, image_format='png'):
    """
    将PPTX文件转换为图片序列
    
    参数:
        pptx_path: PPTX文件路径
        output_folder: 输出文件夹路径
        image_format: 输出图片格式 (默认: png)
    """
    # 确保输出文件夹存在
    if not os.path.exists(output_folder):
        os.makedirs(output_folder)
    
    try:
        # 初始化PowerPoint应用程序
        powerpoint = comtypes.client.CreateObject("PowerPoint.Application")
        powerpoint.Visible = True
        
        # 获取当前PowerPoint版本
        version = powerpoint.Version
        print(f"PowerPoint版本: {version}")
        
        # 打开演示文稿
        print(f"正在打开文件: {pptx_path}")
        presentation = powerpoint.Presentations.Open(os.path.abspath(pptx_path))
        
        # 获取幻灯片数量
        slide_count = presentation.Slides.Count
        print(f"开始转换 {slide_count} 张幻灯片...")
        
        # 遍历每一张幻灯片
        for i in range(1, slide_count + 1):
            try:
                # 导出当前幻灯片为图片
                slide = presentation.Slides(i)
                temp_image_path = os.path.join(output_folder, f"temp_slide_{i:03d}.{image_format}")
                final_image_path = os.path.join(output_folder, f"slide_{i:03d}.{image_format}")
                
                # 导出为图片
                slide.Export(temp_image_path, image_format.upper())
                
                # 使用PIL旋转图片
                with Image.open(temp_image_path) as img:
                    # 顺时针旋转90度
                    rotated_img = img.rotate(-90, expand=True)
                    # 保存旋转后的图片
                    rotated_img.save(final_image_path)
                
                # 删除临时文件
                os.remove(temp_image_path)
                
                print(f"已转换并旋转第 {i}/{slide_count} 张幻灯片")
                
                # 添加短暂延迟，避免系统负载过高
                time.sleep(0.1)
                
            except Exception as slide_error:
                print(f"转换第 {i} 张幻灯片时出错: {str(slide_error)}")
                continue
        
        # 关闭演示文稿
        presentation.Close()
        
        print(f"\n转换完成！图片已保存到: {output_folder}")
        return True
        
    except Exception as e:
        print(f"转换过程中出现错误: {str(e)}")
        print("错误类型:", type(e).__name__)
        print("错误详情:", str(e))
        return False
    
    finally:
        # 退出PowerPoint
        try:
            powerpoint.Quit()
        except:
            pass

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
    os.system('taskkill /f /im POWERPNT.EXE')
    time.sleep(1)
    
    success = convert_pptx_to_images(pptx_path, output_folder)
    sys.exit(0 if success else 1)

if __name__ == "__main__":
    main() 