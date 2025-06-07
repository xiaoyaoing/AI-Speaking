#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Citizens PRO 人物模型清理脚本
每种类型只保留四分之一的模型，删除其他模型及对应的Prefab

使用方法:
python delete_citizens_pro.py [--dry-run]
"""

import os
import shutil
import argparse
import random
from pathlib import Path
from collections import defaultdict

class CitizensProModelCleaner:
    def __init__(self, base_path, dry_run=False):
        self.base_path = Path(base_path)
        self.dry_run = dry_run
        self.deleted_count = 0
        self.kept_count = 0
        
        # 定义路径
        self.models_path = self.base_path / "Assets/Citizens PRO/Models"
        self.prefabs_path = self.base_path / "Assets/Citizens PRO/People Prefabs"
        
    def delete_file_or_folder(self, path):
        """删除文件或文件夹"""
        if not path.exists():
            return
            
        if self.dry_run:
            print(f"[DRY RUN] 将删除: {path}")
            self.deleted_count += 1
        else:
            try:
                if path.is_file():
                    path.unlink()
                    print(f"已删除文件: {path}")
                elif path.is_dir():
                    shutil.rmtree(path)
                    print(f"已删除文件夹: {path}")
                self.deleted_count += 1
            except Exception as e:
                print(f"删除失败 {path}: {e}")
    
    def get_model_categories(self):
        """扫描并分类所有人物模型"""
        categories = defaultdict(list)
        
        if not self.models_path.exists():
            print(f"模型文件夹不存在: {self.models_path}")
            return categories
        
        print(f"扫描模型文件夹: {self.models_path}")
        
        # 扫描 People 1.0 和 People 2.0 文件夹
        for version_folder in self.models_path.iterdir():
            if version_folder.is_dir() and version_folder.name.startswith("People"):
                print(f"  扫描版本文件夹: {version_folder.name}")
                
                # 扫描性别文件夹 (Male, Female 等)
                for gender_folder in version_folder.iterdir():
                    if gender_folder.is_dir() and gender_folder.name in ['Male', 'Female', 'Children']:
                        print(f"    扫描性别文件夹: {gender_folder.name}")
                        
                        # 扫描具体的模型文件夹
                        for model_folder in gender_folder.iterdir():
                            if model_folder.is_dir():
                                # 检查是否包含模型文件
                                fbx_files = list(model_folder.glob("*.FBX"))
                                if fbx_files:
                                    category_key = f"{version_folder.name}_{gender_folder.name}"
                                    model_info = {
                                        'path': model_folder,
                                        'name': model_folder.name,
                                        'category': category_key,
                                        'version': version_folder.name,
                                        'gender': gender_folder.name
                                    }
                                    categories[category_key].append(model_info)
                                    print(f"      找到模型: {model_folder.name}")
        
        return categories
    
    def find_corresponding_prefab(self, model_info):
        """查找对应的Prefab文件"""
        model_name = model_info['name']
        gender = model_info['gender']
        
        # 在对应的性别文件夹中查找prefab
        gender_prefab_folder = self.prefabs_path / gender
        
        if not gender_prefab_folder.exists():
            return None
        
        # 查找匹配的prefab文件
        for prefab_file in gender_prefab_folder.glob("*.prefab"):
            # 检查prefab名称是否与模型名称相关
            if model_name.lower() in prefab_file.stem.lower() or prefab_file.stem.lower() in model_name.lower():
                return prefab_file
        
        # 如果没找到精确匹配，查找包含部分名称的
        model_base_name = model_name.split('_')[0]  # 取第一部分作为基础名称
        for prefab_file in gender_prefab_folder.glob("*.prefab"):
            if model_base_name.lower() in prefab_file.stem.lower():
                return prefab_file
        
        return None
    
    def clean_models(self):
        """清理模型文件，每种类型只保留四分之一"""
        print("\n开始清理人物模型...")
        
        categories = self.get_model_categories()
        
        if not categories:
            print("没有找到任何模型分类")
            return
        
        print(f"\n找到 {len(categories)} 个模型分类:")
        for category, models in categories.items():
            print(f"  {category}: {len(models)} 个模型")
        
        # 对每个分类进行处理
        for category, models in categories.items():
            print(f"\n处理分类: {category}")
            print(f"  总共 {len(models)} 个模型")
            
            if len(models) <= 1:
                print(f"  该分类模型数量过少，全部保留")
                self.kept_count += len(models)
                continue
            
            # 计算要保留的数量（至少保留1个）
            keep_count = max(1, len(models) // 4)
            print(f"  将保留 {keep_count} 个模型，删除 {len(models) - keep_count} 个模型")
            
            # 随机选择要保留的模型
            random.shuffle(models)
            models_to_keep = models[:keep_count]
            models_to_delete = models[keep_count:]
            
            # 删除不需要的模型
            for model_info in models_to_delete:
                print(f"    [删除] {model_info['name']}")
                
                # 删除模型文件夹
                self.delete_file_or_folder(model_info['path'])
                
                # 删除对应的.meta文件
                meta_file = model_info['path'].parent / f"{model_info['path'].name}.meta"
                self.delete_file_or_folder(meta_file)
                
                # 查找并删除对应的prefab
                prefab_file = self.find_corresponding_prefab(model_info)
                if prefab_file:
                    print(f"      删除对应的Prefab: {prefab_file.name}")
                    self.delete_file_or_folder(prefab_file)
                    # 删除prefab的.meta文件
                    prefab_meta = prefab_file.parent / f"{prefab_file.name}.meta"
                    self.delete_file_or_folder(prefab_meta)
                else:
                    print(f"      未找到对应的Prefab文件")
            
            # 统计保留的模型
            for model_info in models_to_keep:
                print(f"    [保留] {model_info['name']}")
                self.kept_count += 1
    
    def clean_empty_folders(self):
        """清理空文件夹"""
        print("\n清理空文件夹...")
        
        def remove_empty_folders(path):
            """递归删除空文件夹"""
            if not path.exists() or not path.is_dir():
                return
            
            # 先处理子文件夹
            for subfolder in path.iterdir():
                if subfolder.is_dir():
                    remove_empty_folders(subfolder)
            
            # 检查当前文件夹是否为空（只包含.meta文件也算空）
            contents = list(path.iterdir())
            non_meta_contents = [item for item in contents if not item.name.endswith('.meta')]
            
            if not non_meta_contents:
                print(f"删除空文件夹: {path}")
                self.delete_file_or_folder(path)
                # 删除对应的.meta文件
                meta_file = path.parent / f"{path.name}.meta"
                self.delete_file_or_folder(meta_file)
        
        # 清理模型文件夹中的空文件夹
        if self.models_path.exists():
            remove_empty_folders(self.models_path)
        
        # 清理prefab文件夹中的空文件夹
        if self.prefabs_path.exists():
            remove_empty_folders(self.prefabs_path)

    def run(self):
        """执行清理"""
        print(f"Citizens PRO 人物模型清理工具")
        print(f"基础路径: {self.base_path}")
        print(f"模式: {'预览模式' if self.dry_run else '实际删除'}")
        print("=" * 50)
        
        # 设置随机种子以确保结果可重现
        random.seed(42)
        
        # 清理模型
        self.clean_models()
        
        # 清理空文件夹
        self.clean_empty_folders()
        
        print("\n" + "=" * 50)
        print(f"清理完成!")
        print(f"删除项目: {self.deleted_count}")
        print(f"保留项目: {self.kept_count}")
        
        if self.dry_run:
            print("\n这是预览模式，没有实际删除任何文件。")
            print("要执行实际删除，请运行: python delete_citizens_pro.py")

def main():
    parser = argparse.ArgumentParser(description='Citizens PRO 人物模型清理工具')
    parser.add_argument('--dry-run', action='store_true', 
                       help='预览模式，不实际删除文件')
    parser.add_argument('--base-path', type=str, default='.',
                       help='项目根目录路径 (默认: 当前目录)')
    
    args = parser.parse_args()
    
    # 验证路径
    base_path = Path(args.base_path)
    if not base_path.exists():
        print(f"错误: 路径不存在: {base_path}")
        return
    
    citizens_path = base_path / "Assets/Citizens PRO"
    if not citizens_path.exists():
        print(f"错误: 找不到 Citizens PRO 资源: {citizens_path}")
        return
    
    # 执行清理
    cleaner = CitizensProModelCleaner(base_path, args.dry_run)
    cleaner.run()

if __name__ == "__main__":
    main() 