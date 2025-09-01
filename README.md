# NCKeys 1.0.0

**NCKeys** is a lightweight offline security utility built with **.NET 9 / C#**, designed to protect your keyboard input, monitor suspicious processes, and optionally safeguard your clipboard. Ideal for enhancing privacy and defending against user-mode keyloggers and spyware.

---

## Features

### 1. Keylogging Protection
- **Low-Level Keyboard Hook:** Captures keystrokes in real time to detect suspicious activity.  
- **Privacy Mode:** Option to ignore keystrokes completely to prevent accidental logging.  
- **Encrypted Keystrokes:** All captured keys are encrypted using AES-256.  
- **Hook Auto-Recovery:** Detects if the hook is removed and automatically re-applies it.  

### 2. Process Monitoring & Anti-Spy
- **Suspicious Process Detection:**  
  - Flags processes with names like `keylogger`, `logger`, `hook`.  
  - Checks hidden windows with high CPU/memory usage.  
  - Detects processes with active **ESTABLISHED** TCP connections.  
  - Validates digital signatures against a whitelist of trusted publishers.  
  - Optional hash validation against known good process hashes.  
- **Trusted Processes & Directories Whitelist:** Avoids false positives for system and common apps.  

### 3. Clipboard Protection
- **Real-Time Monitoring:** Clears clipboard if sensitive data is detected.  
- **Toggleable:** Can be enabled or disabled via settings.  

### 4. Performance & Reliability
- **Real-Time Scanning:** Scans running processes every **2 seconds**.  
- Maintains a list of already scanned PIDs for efficiency.  
- Separate timers for keyboard hook recovery and real-time scanning.  
- **Offline Operation:** Core protection works without network connectivity.  
- **Safe Resource Use:** Optimized to avoid heavy CPU/memory usage.  

### 5. Security & Privacy Features
- **AES Encryption** for key data.  
- **Trusted Publisher Verification** to allow only verified executables to bypass checks.  
- **TCP Connection Filtering:** Focused on **ESTABLISHED** connections to reduce false positives.  

### 6. Developer & User Integration
- **Event Callbacks:**  
  - `OnEncryptedKey`: Returns encrypted keystrokes for internal monitoring.  
  - `OnSuspiciousProcessDetected`: Notifies about detected suspicious processes.  
- **Extensible:** Add more trusted processes, directories, or known hashes.  

### 7. Known Limitations / Future Improvements
- **User-Mode Only:** Cannot detect kernel-level keyloggers.  
- **Signed Malicious Apps:** Rare signed malware may bypass checks.  
- **Static Heuristics:** CPU/memory thresholds can be evaded by advanced malware.  
- **Caching:** Future versions may cache signature/hash checks for performance.  

---

## Supported Threats / Apps NCKeys Can Block or Detect
- User-mode keyloggers (`keylogger.exe`, `logger.exe`, etc.)  
- Hook-based monitoring tools in user-space  
- Hidden processes consuming high CPU/memory  
- Unsigned or untrusted apps establishing TCP connections  
- Clipboard-stealing tools  

---

## Comparison Table

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

## Installation

1. Clone or download the repository.  
2. Open `NCKeys.sln` in Visual Studio 2022 or later.  
3. Build and run the project targeting **.NET 9**.  

---

## Usage

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
