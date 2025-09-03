# 🛡️ NCKeys – Anti-Keylogger Tool

**Version:** `1.0.0`  
**Platform:** Windows (.NET 9 / C# WinForms)  
**Developer:** Noriel Calingasan  
**Release Date:** September 1, 2025  

**Highlights in v1.0.0**
- **Real-time keylogging protection** using a low-level keyboard hook  
- AES-256 encryption of all captured keystrokes  
- Auto-recovery of keyboard hook if removed  
- Suspicious process detection (TCP connections, hidden windows, unsigned apps)  
- Optional clipboard protection to block sensitive data  
- Lightweight offline operation with minimal CPU/memory usage  

---

## 📂 Repository  
Explore source code, report issues, or contribute:  
🔗 [https://github.com/norielcalingasan/NCKeys](https://github.com/norielcalingasan/NCKeys)

---

## 🚀 Features

### Keylogging Protection
- Low-Level Keyboard Hook for real-time keystroke monitoring  
- Privacy Mode to ignore keystrokes completely  
- Encrypted keystrokes using AES-256  
- Hook auto-recovery in case of removal  

### Process Monitoring & Anti-Spy
- Suspicious process detection: keylogger, logger, hook keywords  
- Hidden windows consuming high CPU/memory  
- Detects processes with **ESTABLISHED** TCP connections  
- Digital signature validation against trusted publishers  
- Optional hash validation against known good process hashes  
- Trusted processes & directories whitelist to avoid false positives  

### Clipboard Protection
- Optional real-time monitoring of clipboard  
- Clears sensitive data automatically  
- Toggleable via settings  

### Performance & Reliability
- Real-time scanning of running processes every **2 seconds**  
- Tracks scanned PIDs for efficiency  
- Separate timers for keyboard hook recovery and scanning  
- Fully offline operation, no internet required  
- Minimal CPU and memory usage  

### Developer & User Integration
- Event callbacks:  
  - `OnEncryptedKey`: Returns encrypted keystrokes  
  - `OnSuspiciousProcessDetected`: Notifies about suspicious processes  
- Extensible: Add trusted processes, directories, or known hashes  

---
# ⚖️ Terms of Use

**Important:** By using NCKeys, you agree to the following terms:

---

## 1. Personal & Authorized Use Only

NCKeys is intended for personal security, privacy protection, and educational purposes. Do **not** install or run this software on devices you do not own or have explicit permission to monitor.

---

## 2. Consent Required

You must obtain explicit consent from any other user if NCKeys is installed on a shared or third-party device.

---

## 3. Legal Compliance

You are responsible for complying with all applicable laws regarding computer monitoring, keylogging, and data privacy in your country or region.

---

## 4. No Malicious Use

Using NCKeys to steal passwords, personal data, or other sensitive information is **strictly prohibited** and may lead to criminal or civil liability.

---

## 5. Disclaimer of Liability

NCKeys is provided “as-is” without warranties of any kind. The developer is **not responsible** for any misuse, damages, or legal consequences arising from your use of this software.

---

## 6. Intended Audience

NCKeys is suitable for security enthusiasts, IT professionals, and developers learning about process monitoring and keylogging protection. It is **not a hacking tool**.

---

## 🆚 NCKeys vs Other Security Tools

| Feature                          | NCKeys 1.0.0 | Traditional Anti-Keyloggers | Online AV/Antivirus |
|----------------------------------|--------------|----------------------------|-------------------|
| Real-Time Keylogging Protection   | ✅            | ✅                          | ✅                 |
| AES Encryption of Keystrokes      | ✅            | ❌                          | ❌                 |
| Process Monitoring                | ✅            | Limited                    | ✅                 |
| Clipboard Protection              | ✅ (optional) | ❌                          | ✅                 |
| Offline Operation                 | ✅            | ❌                          | ❌                 |
| User-Mode Only                    | ✅            | ✅                          | ❌ (kernel-level) |
| Hook Auto-Recovery                | ✅            | ❌                          | ❌                 |
| Trusted Publisher Verification    | ✅            | ❌                          | ✅                 |

---

## 🖥️ Installation

1. 📥 Download the repository or clone using Git  
2. 📂 Open `NCKeys.sln` in Visual Studio 2022 or later  
3. ▶️ Build and run the project targeting **.NET 9**  

No installation required. Fully offline and portable.  

---

## 🔧 Usage

```csharp
// Start protection
KeyInterceptor.Start();

// Subscribe to encrypted key event
KeyInterceptor.OnEncryptedKey += (encrypted) =>
{
    Console.WriteLine($"Encrypted Key: {encrypted}");
};

// Subscribe to suspicious process detection
KeyInterceptor.OnSuspiciousProcessDetected += (info) =>
{
    Console.WriteLine($"Suspicious process detected: {info}");
};

// Enable Privacy Mode or Clipboard Protection
KeyInterceptor.PrivacyModeEnabled = true;
KeyInterceptor.ClipboardProtectionEnabled = true;

// Stop protection
KeyInterceptor.Stop();
