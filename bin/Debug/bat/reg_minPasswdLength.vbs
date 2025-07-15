' ========================================
' Script: reg_minPasswdLength.vbs
' Tujuan: Validasi dan pengaturan panjang password minimum
' Author: Windows Security Hardening Tool
' ========================================

On Error Resume Next

' Konstanta
Const HKEY_LOCAL_MACHINE = &H80000002
Const REG_DWORD = 4

' Membuat objek registry
Set objRegistry = GetObject("winmgmts:\\.\root\default:StdRegProv")

' Path registry untuk password policy
strKeyPath = "SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Network"
strValueName = "MinPwdLen"

' Cek apakah key registry ada
objRegistry.GetDWORDValue HKEY_LOCAL_MACHINE, strKeyPath, strValueName, dwValue

If Err.Number = 0 Then
    ' Key ditemukan, periksa nilai
    If IsNull(dwValue) Then
        WScript.Echo "PERINGATAN: Nilai MinPwdLen tidak ditemukan atau kosong"
        WScript.Echo "Mengatur nilai default ke 8 karakter..."
        
        ' Set nilai default
        objRegistry.SetDWORDValue HKEY_LOCAL_MACHINE, strKeyPath, strValueName, 8
        
        If Err.Number = 0 Then
            WScript.Echo "SUKSES: Panjang password minimum berhasil diatur ke 8 karakter"
        Else
            WScript.Echo "ERROR: Gagal mengatur panjang password minimum - " & Err.Description
        End If
    Else
        WScript.Echo "INFO: Panjang password minimum saat ini: " & dwValue & " karakter"
        
        If dwValue < 8 Then
            WScript.Echo "PERINGATAN: Panjang password terlalu pendek (kurang dari 8 karakter)"
            WScript.Echo "Meningkatkan ke 8 karakter..."
            
            objRegistry.SetDWORDValue HKEY_LOCAL_MACHINE, strKeyPath, strValueName, 8
            
            If Err.Number = 0 Then
                WScript.Echo "SUKSES: Panjang password minimum ditingkatkan ke 8 karakter"
            Else
                WScript.Echo "ERROR: Gagal meningkatkan panjang password - " & Err.Description
            End If
        ElseIf dwValue >= 8 Then
            WScript.Echo "SUKSES: Panjang password sudah memenuhi standar keamanan"
        End If
    End If
Else
    WScript.Echo "ERROR: Tidak dapat mengakses registry - " & Err.Description
    WScript.Echo "Pastikan script dijalankan dengan hak akses administrator"
End If

' Validasi tambahan untuk Local Security Policy
WScript.Echo ""
WScript.Echo "=== VALIDASI LOCAL SECURITY POLICY ==="

' Cek GPO password policy
Set objShell = CreateObject("WScript.Shell")
Set objExec = objShell.Exec("net accounts")

strOutput = ""
Do While Not objExec.StdOut.AtEndOfStream
    strOutput = strOutput & objExec.StdOut.ReadLine() & vbCrLf
Loop

If InStr(strOutput, "Minimum password length") > 0 Then
    WScript.Echo "INFO: Checking Local Security Policy..."
    
    ' Extract minimum password length from net accounts output
    arrLines = Split(strOutput, vbCrLf)
    For i = 0 To UBound(arrLines)
        If InStr(arrLines(i), "Minimum password length") > 0 Then
            WScript.Echo "LOCAL POLICY: " & Trim(arrLines(i))
            Exit For
        End If
    Next
Else
    WScript.Echo "PERINGATAN: Tidak dapat membaca Local Security Policy"
End If

WScript.Echo ""
WScript.Echo "=== REKOMENDASI KEAMANAN ==="
WScript.Echo "1. Pastikan password menggunakan kombinasi huruf besar, kecil, angka, dan simbol"
WScript.Echo "2. Hindari penggunaan kata-kata umum atau informasi pribadi"
WScript.Echo "3. Ganti password secara berkala"
WScript.Echo "4. Aktifkan account lockout policy untuk mencegah brute force"

' Cleanup
Set objRegistry = Nothing
Set objShell = Nothing

WScript.Echo ""
WScript.Echo "Script validasi password selesai."
