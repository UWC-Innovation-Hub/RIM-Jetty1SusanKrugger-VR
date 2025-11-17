================================================================================
META QUEST ADB TROUBLESHOOTING GUIDE
Fixing "4 Authorization Requests" Issue
================================================================================

PROBLEM DESCRIPTION:
-------------------
When switching from Unity XR Interaction to Meta XR SDK (v78.0.0), the Quest 2
device shows 4 quick authorization requests instead of 1 like other machines.

ADB is working, device is seen, but the authorization process is happening
multiple times.

ROOT CAUSE:
----------
Multiple ADB (Android Debug Bridge) servers are running simultaneously from:
  • Unity's Android SDK
  • Meta Quest Developer Hub (MQDH)
  • Android Studio (if installed)
  • Meta XR SDK build tools

Each ADB server instance attempts to authorize independently, causing the 
4 authorization requests you're seeing in the headset.

WHY IT WORKS WITH UNITY XR BUT NOT META SDK:
--------------------------------------------
Unity XR Interaction uses Unity's standard build pipeline with a single ADB
instance. Meta XR SDK may spawn additional ADB connections through:
  • Meta-specific build hooks
  • OVR build tools
  • Meta Quest Developer Hub integration
  • Additional Android SDK paths

SOLUTION:
---------
Use the "Meta > ADB Conflict Resolver" tool to detect and fix this issue.

STEP-BY-STEP FIX:
----------------
1. Open Unity Editor
2. Go to Menu: Meta > ADB Conflict Resolver
3. Click "Quick Fix: Kill All & Restart" button
   
   OR follow manual steps:
   
   a) Click "1. Diagnose Multiple ADB Instances"
      - This shows how many ADB installations exist
      - Shows how many are currently running
      
   b) Click "2. Kill ALL ADB Servers"
      - Terminates ALL ADB servers (Unity, MQDH, Android Studio)
      - Force kills any remaining processes
      
   c) IMPORTANT: Close Meta Quest Developer Hub if it's running
      - MQDH will restart its own ADB server if left open
      
   d) Click "3. Start Single ADB Server"
      - Starts only Unity's ADB server
      
   e) Reconnect Quest headset:
      - Unplug USB cable
      - Wait 3 seconds
      - Plug back in
      
   f) Accept the SINGLE authorization request in headset
      - Make sure to check "Always allow from this computer"

4. Verify with "4. Verify Single Server Running"
   - Should show only 1 ADB process
   
5. Build and Run from Unity
   - Should now work without 4 authorization prompts

PREVENTING FUTURE ISSUES:
-------------------------
• Keep Meta Quest Developer Hub closed while building from Unity
• If using Android Studio, close it during Unity builds
• Use the "Check for Multiple ADB Instances" button in 
  "Meta > Quest Connection Helper" to verify before building

ALTERNATIVE TOOLS PROVIDED:
---------------------------
1. Meta > Quest Connection Helper
   - Check device status
   - Restart ADB server
   - Clear authorization keys
   - Check for multiple ADB instances
   
2. Meta > XR Settings Validator
   - Validates project settings for Meta Quest
   - One-click fixes for common configuration issues
   
3. Meta > Quick Actions
   - Quick access to Unity settings
   - Links to documentation
   - Launch Meta Quest Developer Hub

COMMON ERRORS RESOLVED:
-----------------------
✓ Multiple authorization requests (4 instead of 1)
✓ Device showing as "unauthorized" after accepting prompt
✓ "Unable to install APK to device" errors
✓ Device appearing then disappearing from Unity device list
✓ Build succeeds but app doesn't launch on Quest

TECHNICAL DETAILS:
-----------------
Each ADB server maintains its own authorization database. When multiple
servers run simultaneously:
  
  Server 1 (Unity): Requests authorization
  Server 2 (MQDH): Requests authorization  
  Server 3 (Android Studio): Requests authorization
  Server 4 (Meta SDK): Requests authorization
  
Result: 4 rapid-fire authorization prompts in the headset

By consolidating to a single ADB server instance, you reduce this to
one authorization request, as expected.

VERIFICATION COMMANDS (Optional - for advanced users):
----------------------------------------------------
You can also verify from command line:

Windows:
  tasklist | findstr adb

macOS/Linux:
  ps aux | grep adb

Should show only ONE adb process after using the fix.

ADDITIONAL NOTES:
----------------
• This issue is machine-specific because different setups have different
  combinations of tools installed (Android Studio, MQDH, etc.)
  
• Other machines working fine likely have either:
  - Only Unity's Android SDK installed
  - Meta Quest Developer Hub not installed/not running
  - Different Android SDK configuration
  
• The issue manifests specifically with Meta SDK because it may trigger
  additional Android build processes that Unity XR doesn't use

TROUBLESHOOTING:
---------------
If the fix doesn't work:

1. Ensure ALL these applications are CLOSED:
   - Meta Quest Developer Hub
   - Android Studio
   - Any ADB command windows
   
2. Restart your computer (clean slate)

3. After restart, run the Quick Fix again BEFORE opening any other tools

4. Try different USB cable/port (still the #1 cause of issues)

5. Check that Developer Mode is enabled:
   - Open Meta Quest mobile app
   - Go to Menu > Devices > [Your Quest] > Developer Mode
   - Enable if not already on

SUPPORT:
--------
For additional help:
• Meta Quest Developer Forums: https://communityforums.atmeta.com
• Unity XR Forum: https://discussions.unity.com
• Meta Developer Documentation: https://developer.oculus.com

================================================================================
Tools created by: Meta Quest Connection Helper Scripts
Last updated: 2024
================================================================================
