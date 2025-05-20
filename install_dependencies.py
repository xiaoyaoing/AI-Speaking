import subprocess
import sys

def install_dependencies():
    """安装必要的Python库"""
    print("开始安装必要的Python库...")
    
    # 需要安装的库列表
    packages = [
        'comtypes'
    ]
    
    # 使用pip安装每个库
    for package in packages:
        print(f"\n正在安装 {package}...")
        try:
            subprocess.check_call([sys.executable, "-m", "pip", "install", package])
            print(f"{package} 安装成功！")
        except subprocess.CalledProcessError:
            print(f"{package} 安装失败！")
            return False
    
    print("\n所有依赖安装完成！")
    return True

if __name__ == "__main__":
    install_dependencies()
    input("\n按回车键退出...") 