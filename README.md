# 🛡️ NCKeys – Anti-Keylogger Tool

**Version:** `1.1.0`  
**Platform:** Windows (.NET 9 / C# WinForms)  
**Developer:** Noriel Calingasan  
**Release Date:** September 1, 2025  
**Update Date:** September 5, 2025  

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
- Real-time scanning of running processes every **2–5 seconds**  
- Tracks scanned PIDs for efficiency  
- Incremental process scanning in batches  
- Separate timers for keyboard hook recovery and scanning  
- Fully offline operation, no internet required  
- Minimal CPU and memory usage  

### Developer & User Integration
- Event callbacks:  
  - `OnEncryptedKey`: Returns encrypted keystrokes  
  - `OnSuspiciousProcessDetected`: Notifies about suspicious processes  
- Extensible: Add trusted processes, directories, or known hashes  

---

# ⚖️ Terms of Use – NCKeys

**Important:** By using NCKeys, you acknowledge and agree to these terms. If you do not agree, do **not** use this software.

---

## 1. Personal & Authorized Use Only

NCKeys is intended solely for personal security, privacy protection, and educational purposes.  
Do **not** install, run, or use this software on devices you do not own or do not have explicit written permission to monitor.

---

## 2. Consent Requirement

Before using NCKeys on any shared, public, or third-party device, you must obtain **explicit consent** from all users. Unauthorized use may violate local, national, or international laws.

---

## 3. Legal Compliance

You are solely responsible for complying with all applicable laws regarding computer monitoring, keylogging, data privacy, and cybersecurity in your jurisdiction.  
This includes, but is not limited to:

- Republic Act No. 10173 – Data Privacy Act of 2012 (Philippines)  
- Republic Act No. 8792 – E-Commerce Act  
- Republic Act No. 10175 – Cybercrime Prevention Act of 2012  
- Relevant local and international computer misuse or privacy laws  

---

## 4. Prohibited Use

NCKeys must **never** be used to:

- Steal passwords, credentials, or any personally identifiable information  
- Intercept private communications without consent  
- Monitor or spy on third-party devices without authorization  

Violations may lead to **criminal or civil liability** under applicable laws.

---

## 5. No Warranty / Disclaimer of Liability

NCKeys is provided “as-is,” without warranties of any kind, either express or implied.  
The developer, Noriel Calingasan, and associated parties are **not responsible** for:

- Any misuse of the software  
- Loss or theft of data  
- Legal consequences arising from your actions  
- Any damages to hardware or software resulting from use  

---

## 6. Intended Audience

NCKeys is intended for:

- Security enthusiasts  
- IT professionals  
- Developers learning about process monitoring and keylogging protection  

It is explicitly **not a hacking tool**. Users must use it **ethically and legally**.

---

## 7. Acceptance

By installing or running NCKeys, you acknowledge that you have **read, understood, and agreed** to these terms.  
If you do not agree, immediately uninstall and discontinue use.

---

## 🆚 NCKeys vs Other Security Tools

| Feature                          | NCKeys 1.1.0 | Traditional Anti-Keyloggers | Online AV/Antivirus |
|----------------------------------|--------------|----------------------------|-------------------|
| Real-Time Keylogging Protection   | ✅            | ✅                          | ✅                 |
| AES Encryption of Keystrokes      | ✅            | ❌                          | ❌                 |
| Process Monitoring                | ✅            | Limited                    | ✅                 |
| Clipboard Protection              | ✅ (optional) | ❌                          | ✅                 |
| Offline Operation                 | ✅            | ❌                          | ❌                 |
| User-Mode Only                    | ✅            | ✅                          | ❌ (kernel-level) |
| Hook Auto-Recovery                | ✅            | ❌                          | ❌                 |
| Trusted Publisher Verification    | ✅            | ❌                          | ✅                 |
| Memory Usage Monitor              | ✅            | ❌                          | ❌                 |
| Terms of Use Enforcement          | ✅            | ❌                          | ❌                 |

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
Properties.Settings.Default.PrivacyMode = true;
Properties.Settings.Default.ClipboardProtection = true;

// Stop protection
KeyInterceptor.Stop();
