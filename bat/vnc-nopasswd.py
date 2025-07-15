#!/usr/bin/env python3
"""
========================================
Script: vnc-nopasswd.py
Purpose: VNC Password Security Audit
Author: Windows Security Hardening Tool
========================================
"""

import os
import sys
import winreg
import subprocess
import socket
import json
from pathlib import Path
import psutil

class VNCSecurityAuditor:
    def __init__(self):
        self.vulnerabilities = []
        self.vnc_ports = [5900, 5901, 5902, 5903, 5904, 5905]
        self.common_vnc_paths = [
            r"C:\Program Files\RealVNC",
            r"C:\Program Files (x86)\RealVNC", 
            r"C:\Program Files\TightVNC",
            r"C:\Program Files (x86)\TightVNC",
            r"C:\Program Files\UltraVNC",
            r"C:\Program Files (x86)\UltraVNC",
            r"C:\VNC",
            r"C:\TightVNC"
        ]
        
    def print_header(self):
        print("=" * 50)
        print("    VNC PASSWORD SECURITY AUDITOR")
        print("=" * 50)
        print()
        
    def check_vnc_processes(self):
        """Check for running VNC processes"""
        print("🔍 MEMERIKSA PROSES VNC YANG BERJALAN...")
        print("-" * 40)
        
        vnc_processes = []
        vnc_keywords = ['vnc', 'tightvnc', 'realvnc', 'ultravnc', 'winvnc']
        
        for proc in psutil.process_iter(['pid', 'name', 'cmdline']):
            try:
                proc_name = proc.info['name'].lower() if proc.info['name'] else ""
                cmdline = ' '.join(proc.info['cmdline']).lower() if proc.info['cmdline'] else ""
                
                if any(keyword in proc_name or keyword in cmdline for keyword in vnc_keywords):
                    vnc_processes.append({
                        'pid': proc.info['pid'],
                        'name': proc.info['name'],
                        'cmdline': proc.info['cmdline']
                    })
                    print(f"   ⚠️  DITEMUKAN: {proc.info['name']} (PID: {proc.info['pid']})")
                    
            except (psutil.NoSuchProcess, psutil.AccessDenied):
                continue
                
        if not vnc_processes:
            print("   ✅ Tidak ada proses VNC yang terdeteksi berjalan")
        else:
            self.vulnerabilities.append("VNC processes detected running")
            
        print()
        return vnc_processes
        
    def check_vnc_ports(self):
        """Check for open VNC ports"""
        print("🌐 MEMERIKSA PORT VNC YANG TERBUKA...")
        print("-" * 40)
        
        open_ports = []
        
        for port in self.vnc_ports:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(1)
            
            try:
                result = sock.connect_ex(('127.0.0.1', port))
                if result == 0:
                    open_ports.append(port)
                    print(f"   ⚠️  PORT {port} TERBUKA - Kemungkinan VNC Server aktif!")
                    
                    # Try to get banner/version info
                    try:
                        sock.send(b'RFB 003.008\n')
                        response = sock.recv(1024)
                        if response:
                            print(f"      📋 Response: {response[:50]}")
                    except:
                        pass
                        
            except socket.error:
                pass
            finally:
                sock.close()
                
        if not open_ports:
            print("   ✅ Tidak ada port VNC yang terbuka")
        else:
            self.vulnerabilities.append(f"Open VNC ports detected: {open_ports}")
            
        print()
        return open_ports
        
    def check_vnc_registry(self):
        """Check VNC-related registry entries"""
        print("📝 MEMERIKSA REGISTRY VNC...")
        print("-" * 40)
        
        registry_keys = [
            (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\RealVNC"),
            (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\TightVNC"),  
            (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\UltraVNC"),
            (winreg.HKEY_CURRENT_USER, r"SOFTWARE\RealVNC"),
            (winreg.HKEY_CURRENT_USER, r"SOFTWARE\TightVNC"),
            (winreg.HKEY_CURRENT_USER, r"SOFTWARE\UltraVNC"),
        ]
        
        found_entries = []
        
        for hkey, subkey in registry_keys:
            try:
                with winreg.OpenKey(hkey, subkey) as key:
                    # Get basic info about the key
                    num_subkeys, num_values, last_modified = winreg.QueryInfoKey(key)
                    
                    hkey_name = "HKLM" if hkey == winreg.HKEY_LOCAL_MACHINE else "HKCU"
                    print(f"   📁 DITEMUKAN: {hkey_name}\\{subkey}")
                    print(f"      Subkeys: {num_subkeys}, Values: {num_values}")
                    
                    # Check for password-related values
                    for i in range(num_values):
                        try:
                            value_name, value_data, value_type = winreg.EnumValue(key, i)
                            
                            if 'password' in value_name.lower() or 'passwd' in value_name.lower():
                                if value_data == "" or value_data is None:
                                    print(f"      ⚠️  PASSWORD KOSONG: {value_name}")
                                    self.vulnerabilities.append(f"Empty VNC password in registry: {subkey}\\{value_name}")
                                else:
                                    print(f"      🔒 Password ditemukan: {value_name} (encrypted)")
                                    
                        except WindowsError:
                            continue
                            
                    found_entries.append(subkey)
                    
            except WindowsError:
                continue
                
        if not found_entries:
            print("   ✅ Tidak ada entri VNC di registry")
        
        print()
        return found_entries
        
    def check_vnc_config_files(self):
        """Check VNC configuration files"""
        print("📄 MEMERIKSA FILE KONFIGURASI VNC...")
        print("-" * 40)
        
        config_files = []
        
        # Check common VNC installation paths
        for vnc_path in self.common_vnc_paths:
            if os.path.exists(vnc_path):
                print(f"   📁 VNC Installation ditemukan: {vnc_path}")
                
                # Look for config files
                for root, dirs, files in os.walk(vnc_path):
                    for file in files:
                        if file.lower().endswith(('.ini', '.conf', '.config', '.reg')):
                            config_file = os.path.join(root, file)
                            config_files.append(config_file)
                            print(f"      📄 Config file: {file}")
                            
                            # Check file contents for password issues
                            try:
                                with open(config_file, 'r', encoding='utf-8', errors='ignore') as f:
                                    content = f.read().lower()
                                    
                                    if 'password=' in content and ('password=""' in content or 'password=\n' in content):
                                        print(f"         ⚠️  EMPTY PASSWORD DETECTED!")
                                        self.vulnerabilities.append(f"Empty password in config: {config_file}")
                                        
                                    if 'authentication=0' in content or 'auth=0' in content:
                                        print(f"         ⚠️  AUTHENTICATION DISABLED!")
                                        self.vulnerabilities.append(f"Authentication disabled: {config_file}")
                                        
                            except Exception as e:
                                print(f"         ❌ Error reading file: {e}")
                                
        # Check user profile directories
        user_dirs = [
            os.path.expanduser("~"),
            r"C:\Users\Default",
            r"C:\Users\Administrator"
        ]
        
        for user_dir in user_dirs:
            vnc_config_paths = [
                os.path.join(user_dir, ".vnc"),
                os.path.join(user_dir, "AppData", "Roaming", "RealVNC"),
                os.path.join(user_dir, "AppData", "Local", "RealVNC")
            ]
            
            for config_path in vnc_config_paths:
                if os.path.exists(config_path):
                    print(f"   📁 User VNC config: {config_path}")
                    
        if not config_files:
            print("   ✅ Tidak ada file konfigurasi VNC yang ditemukan")
            
        print()
        return config_files
        
    def check_windows_remote_desktop(self):
        """Check Windows Remote Desktop settings"""
        print("🖥️  MEMERIKSA WINDOWS REMOTE DESKTOP...")
        print("-" * 40)
        
        try:
            # Check if RDP is enabled
            with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, 
                               r"SYSTEM\CurrentControlSet\Control\Terminal Server") as key:
                
                fDenyTSConnections, _ = winreg.QueryValueEx(key, "fDenyTSConnections")
                
                if fDenyTSConnections == 0:
                    print("   ⚠️  Windows Remote Desktop ENABLED")
                    
                    # Check authentication settings
                    try:
                        with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE,
                                           r"SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp") as rdp_key:
                            
                            try:
                                UserAuthentication, _ = winreg.QueryValueEx(rdp_key, "UserAuthentication")
                                if UserAuthentication == 0:
                                    print("   ⚠️  RDP Network Level Authentication DISABLED")
                                    self.vulnerabilities.append("RDP NLA disabled")
                                else:
                                    print("   ✅ RDP Network Level Authentication enabled")
                            except FileNotFoundError:
                                print("   ⚠️  RDP UserAuthentication setting not found")
                                
                    except WindowsError as e:
                        print(f"   ❌ Error checking RDP settings: {e}")
                        
                else:
                    print("   ✅ Windows Remote Desktop is disabled")
                    
        except WindowsError as e:
            print(f"   ❌ Error checking Remote Desktop: {e}")
            
        print()
        
    def generate_report(self):
        """Generate security report"""
        print("📊 LAPORAN KEAMANAN VNC")
        print("=" * 50)
        
        if not self.vulnerabilities:
            print("✅ EXCELLENT: Tidak ada kerentanan VNC yang ditemukan!")
            print("   Sistem Anda sudah mengikuti praktik keamanan yang baik.")
        else:
            print(f"⚠️  PERINGATAN: {len(self.vulnerabilities)} kerentanan ditemukan:")
            print()
            
            for i, vuln in enumerate(self.vulnerabilities, 1):
                print(f"{i}. {vuln}")
                
            print()
            print("🔧 REKOMENDASI PERBAIKAN:")
            print("-" * 30)
            print("1. Disable atau uninstall VNC jika tidak diperlukan")
            print("2. Gunakan password yang kuat untuk VNC")
            print("3. Aktifkan enkripsi VNC jika tersedia")  
            print("4. Gunakan VPN untuk akses remote yang aman")
            print("5. Batasi akses VNC hanya dari IP yang dipercaya")
            print("6. Gunakan Windows Remote Desktop dengan NLA sebagai alternatif")
            print("7. Regular audit dan monitoring akses remote")
            
        print()
        print("=" * 50)
        
    def run_audit(self):
        """Run complete VNC security audit"""
        self.print_header()
        
        try:
            self.check_vnc_processes()
            self.check_vnc_ports() 
            self.check_vnc_registry()
            self.check_vnc_config_files()
            self.check_windows_remote_desktop()
            self.generate_report()
            
        except Exception as e:
            print(f"❌ Error during audit: {e}")
            print("Pastikan script dijalankan dengan hak akses administrator")
            
        print("Audit VNC selesai.")
        
if __name__ == "__main__":
    print("Starting VNC Security Audit...")
    print()
    
    auditor = VNCSecurityAuditor()
    auditor.run_audit()
    
    print()
    input("Tekan Enter untuk menutup...")
