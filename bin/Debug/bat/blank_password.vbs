' ========================================
' Script: blank_password.vbs
' Tujuan: Deteksi dan penanganan akun dengan password kosong
' Author: Windows Security Hardening Tool
' ========================================

On Error Resume Next

WScript.Echo "=== PEMINDAI PASSWORD KOSONG ==="
WScript.Echo "Memeriksa akun pengguna dengan password kosong..."
WScript.Echo ""

' Membuat objek untuk mengakses informasi user
Set objComputer = GetObject("winmgmts:\\.\root\cimv2")
Set colUsers = objComputer.ExecQuery("SELECT * FROM Win32_UserAccount WHERE LocalAccount = True")

Dim blankPasswordFound
blankPasswordFound = False
Dim userCount
userCount = 0

WScript.Echo "DAFTAR AKUN PENGGUNA LOKAL:"
WScript.Echo "============================"

For Each objUser In colUsers
    userCount = userCount + 1
    
    WScript.Echo userCount & ". Nama: " & objUser.Name
    WScript.Echo "   SID: " & objUser.SID
    WScript.Echo "   Status: " & IIf(objUser.Disabled, "Nonaktif", "Aktif")
    WScript.Echo "   Lockout: " & IIf(objUser.Lockout, "Ya", "Tidak")
    
    ' Cek apakah password required
    If objUser.PasswordRequired = False Then
        WScript.Echo "   PASSWORD: KOSONG - RISIKO KEAMANAN TINGGI!"
        blankPasswordFound = True
    Else
        WScript.Echo "   PASSWORD: Diperlukan (Aman)"
    End If
    
    ' Cek password expires
    If objUser.PasswordExpires = False Then
        WScript.Echo "   EXPIRY: Password tidak pernah kadaluarsa"
    Else
        WScript.Echo "   EXPIRY: Password akan kadaluarsa"
    End If
    
    WScript.Echo ""
Next

' Registry check untuk password policy
WScript.Echo "=== CEK REGISTRY PASSWORD POLICY ==="
Set objRegistry = GetObject("winmgmts:\\.\root\default:StdRegProv")
Const HKEY_LOCAL_MACHINE = &H80000002

' Cek various password-related registry keys
Dim regChecks(5)
regChecks(0) = Array("SYSTEM\CurrentControlSet\Control\Lsa", "LimitBlankPasswordUse", "Pembatasan Password Kosong")
regChecks(1) = Array("SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Network", "MinPwdLen", "Panjang Password Minimum")
regChecks(2) = Array("SYSTEM\CurrentControlSet\Services\Netlogon\Parameters", "RequireSignOrSeal", "Require Sign/Seal")
regChecks(3) = Array("SYSTEM\CurrentControlSet\Control\Lsa", "NoLMHash", "Disable LM Hash")
regChecks(4) = Array("SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "PasswordExpiryWarning", "Password Expiry Warning")
regChecks(5) = Array("SYSTEM\CurrentControlSet\Control\Lsa", "RestrictAnonymous", "Restrict Anonymous Access")

For i = 0 To UBound(regChecks)
    keyPath = regChecks(i)(0)
    valueName = regChecks(i)(1)
    description = regChecks(i)(2)
    
    objRegistry.GetDWORDValue HKEY_LOCAL_MACHINE, keyPath, valueName, dwValue
    
    If Err.Number = 0 And Not IsNull(dwValue) Then
        WScript.Echo description & ": " & dwValue
    Else
        WScript.Echo description & ": Tidak ditemukan atau error"
    End If
    Err.Clear
Next

WScript.Echo ""

' Rekomendasi berdasarkan temuan
If blankPasswordFound Then
    WScript.Echo "=== TINDAKAN YANG DIPERLUKAN ==="
    WScript.Echo "PERINGATAN: Ditemukan akun dengan password kosong!"
    WScript.Echo ""
    WScript.Echo "LANGKAH PERBAIKAN:"
    WScript.Echo "1. Buka Computer Management (compmgmt.msc)"
    WScript.Echo "2. Pilih Local Users and Groups > Users"
    WScript.Echo "3. Klik kanan pada akun yang bermasalah"
    WScript.Echo "4. Pilih 'Set Password' dan atur password yang kuat"
    WScript.Echo "5. Centang 'User must change password at next logon' jika perlu"
    WScript.Echo ""
    WScript.Echo "ATAU gunakan command line:"
    WScript.Echo "net user [username] [newpassword]"
    WScript.Echo ""
    
    ' Automatic fix untuk registry
    WScript.Echo "=== PERBAIKAN OTOMATIS REGISTRY ==="
    
    ' Set LimitBlankPasswordUse to 1 (limit blank passwords)
    objRegistry.SetDWORDValue HKEY_LOCAL_MACHINE, "SYSTEM\CurrentControlSet\Control\Lsa", "LimitBlankPasswordUse", 1
    If Err.Number = 0 Then
        WScript.Echo "SUKSES: Pembatasan password kosong diaktifkan"
    Else
        WScript.Echo "ERROR: Gagal mengaktifkan pembatasan password kosong - " & Err.Description
    End If
    Err.Clear
    
    ' Ensure minimum password length is set
    objRegistry.SetDWORDValue HKEY_LOCAL_MACHINE, "SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Network", "MinPwdLen", 8
    If Err.Number = 0 Then
        WScript.Echo "SUKSES: Panjang password minimum diatur ke 8 karakter"
    Else
        WScript.Echo "ERROR: Gagal mengatur panjang password minimum - " & Err.Description
    End If
    Err.Clear
    
Else
    WScript.Echo "=== HASIL PEMINDAIAN ==="
    WScript.Echo "BAIK: Tidak ditemukan akun dengan password kosong"
    WScript.Echo "Sistem sudah mengikuti praktik keamanan password yang baik"
End If

WScript.Echo ""
WScript.Echo "=== REKOMENDASI KEAMANAN TAMBAHAN ==="
WScript.Echo "1. Aktifkan Account Lockout Policy (5 percobaan gagal)"
WScript.Echo "2. Set Maximum Password Age (90 hari)"
WScript.Echo "3. Enforce Password History (12 password terakhir)"
WScript.Echo "4. Aktifkan Password Complexity Requirements"
WScript.Echo "5. Gunakan Local Security Policy (secpol.msc) untuk konfigurasi lanjutan"

' Cleanup
Set objComputer = Nothing
Set colUsers = Nothing
Set objRegistry = Nothing

WScript.Echo ""
WScript.Echo "Pemindaian password kosong selesai."

' Helper function for VBScript
Function IIf(condition, trueValue, falseValue)
    If condition Then
        IIf = trueValue
    Else
        IIf = falseValue
    End If
End Function
