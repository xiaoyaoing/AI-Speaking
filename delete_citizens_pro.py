#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Citizens PRO 动画清理脚本
只保留指定的动画文件，删除其他动画

使用方法:
python delete_citizens_pro.py [--dry-run]
"""

import os
import shutil
import argparse
from pathlib import Path

class CitizensProAnimationCleaner:
    def __init__(self, base_path, dry_run=False):
        self.base_path = Path(base_path)
        self.dry_run = dry_run
        self.deleted_count = 0
        self.kept_count = 0
        
        # 定义路径
        self.animations_path = self.base_path / "Assets/Citizens PRO/Animations"
        self.animations_nolegs_path = self.base_path / "Assets/Citizens PRO/Animations_NoLegs"
        
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
    
    def clean_animations(self):
        """清理动画文件，只保留指定的动画"""
        print("\n清理动画文件...")
        
        # 要保留的动画名称（不包含扩展名）
        keep_animations = {
            'talk1', 'talk2', 'walk', 'claphands', 'listen',
            'talk1_f', 'talk2_f', 'walk_f', 'claphands_f', 'listen_f',
            'idle1', 'idle2', 'idle1_f', 'idle2_f', 'sitidle', 'sitidle_f'
        }
        
        # 清理 Animations_NoLegs 文件夹中的动画文件
        self.clean_animation_folder(self.animations_nolegs_path, keep_animations)
        
        # 清理 Animations 文件夹中的FBX文件
        self.clean_animation_folder(self.animations_path, keep_animations)
        
        # 删除儿童动画文件夹
        self.clean_child_animation_folders()
    
    def delete_animations_nolegs(self):
        """删除整个 Animations_NoLegs 文件夹"""
        if self.animations_nolegs_path.exists():
            print(f"\n删除整个文件夹: {self.animations_nolegs_path}")
            self.delete_file_or_folder(self.animations_nolegs_path)
            # 删除对应的.meta文件
            meta_file = self.animations_nolegs_path.parent / f"{self.animations_nolegs_path.name}.meta"
            self.delete_file_or_folder(meta_file)
        else:
            print(f"文件夹不存在: {self.animations_nolegs_path}")
    
    def clean_animation_folder(self, folder_path, keep_animations):
        """清理指定动画文件夹中的动画文件"""
        if not folder_path.exists():
            print(f"动画文件夹不存在: {folder_path}")
            return
        
        print(f"\n处理动画文件夹: {folder_path.name}")
        
        # 遍历所有子文件夹（Man, Girl no Heel, Girl with Heel等）
        for subfolder in folder_path.iterdir():
            if subfolder.is_dir() and subfolder.name not in ['Child Man', 'Child Girl']:
                print(f"  处理子文件夹: {subfolder.name}")
                
                # 根据文件夹类型处理不同的文件格式
                if folder_path.name == "Animations_NoLegs":
                    # Animations_NoLegs 文件夹包含 .anim 文件
                    anim_files = list(subfolder.glob("*.anim"))
                    print(f"    找到 {len(anim_files)} 个ANIM文件")
                    
                    if not anim_files:
                        print(f"    该文件夹中没有ANIM文件")
                        continue
                    
                    for anim_file in anim_files:
                        animation_name = anim_file.stem  # 获取不带扩展名的文件名
                        
                        if animation_name not in keep_animations:
                            print(f"    [删除动画] {animation_name}")
                            # 删除ANIM文件
                            self.delete_file_or_folder(anim_file)
                            # 删除对应的.meta文件
                            meta_file = subfolder / f"{anim_file.name}.meta"
                            self.delete_file_or_folder(meta_file)
                        else:
                            print(f"    [保留动画] {animation_name}")
                            self.kept_count += 1
                else:
                    # Animations 文件夹包含 .FBX 文件
                    fbx_files = list(subfolder.glob("*.FBX"))
                    print(f"    找到 {len(fbx_files)} 个FBX文件")
                    
                    if not fbx_files:
                        print(f"    该文件夹中没有FBX文件")
                        continue
                    
                    for fbx_file in fbx_files:
                        animation_name = fbx_file.stem  # 获取不带扩展名的文件名
                        
                        if animation_name not in keep_animations:
                            print(f"    [删除动画] {animation_name}")
                            # 删除FBX文件
                            self.delete_file_or_folder(fbx_file)
                            # 删除对应的.meta文件
                            meta_file = subfolder / f"{fbx_file.name}.meta"
                            self.delete_file_or_folder(meta_file)
                        else:
                            print(f"    [保留动画] {animation_name}")
                            self.kept_count += 1
    
    def clean_child_animation_folders(self):
        """删除儿童动画文件夹"""
        child_folders = ['Child Man', 'Child Girl']
        
        # 删除 Animations 文件夹中的儿童动画
        if self.animations_path.exists():
            for child_folder_name in child_folders:
                child_folder = self.animations_path / child_folder_name
                if child_folder.exists():
                    print(f"\n删除儿童动画文件夹: {child_folder}")
                    self.delete_file_or_folder(child_folder)
                    # 删除对应的.meta文件
                    meta_file = self.animations_path / f"{child_folder_name}.meta"
                    self.delete_file_or_folder(meta_file)
        
        # 删除 Animations_NoLegs 文件夹中的儿童动画
        if self.animations_nolegs_path.exists():
            for child_folder_name in child_folders:
                child_folder = self.animations_nolegs_path / child_folder_name
                if child_folder.exists():
                    print(f"\n删除儿童动画文件夹: {child_folder}")
                    self.delete_file_or_folder(child_folder)
                    # 删除对应的.meta文件
                    meta_file = self.animations_nolegs_path / f"{child_folder_name}.meta"
                    self.delete_file_or_folder(meta_file)

    def run(self):
        """执行清理"""
        print(f"Citizens PRO 动画清理工具")
        print(f"基础路径: {self.base_path}")
        print(f"模式: {'预览模式' if self.dry_run else '实际删除'}")
        print("=" * 50)
        
        # 清理动画
        self.clean_animations()
        
        print("\n" + "=" * 50)
        print(f"清理完成!")
        print(f"删除项目: {self.deleted_count}")
        print(f"保留项目: {self.kept_count}")
        
        if self.dry_run:
            print("\n这是预览模式，没有实际删除任何文件。")
            print("要执行实际删除，请运行: python delete_citizens_pro.py")

def main():
    parser = argparse.ArgumentParser(description='Citizens PRO 动画清理工具')
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
    cleaner = CitizensProAnimationCleaner(base_path, args.dry_run)
    cleaner.run()

if __name__ == "__main__":
    main() 