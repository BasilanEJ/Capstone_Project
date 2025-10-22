
async function generateEnhancedFingerprint() {
    try {
        const encoder = new TextEncoder();

        // ===========================================
        // HARDWARE-LEVEL IDENTIFIERS (Same across all browsers)
        // ===========================================
        const screenWidth = screen.width;
        const screenHeight = screen.height;
        const screenColorDepth = screen.colorDepth;
        const screenPixelDepth = screen.pixelDepth || screen.colorDepth;
        const hardwareConcurrency = navigator.hardwareConcurrency || 0;
        const deviceMemory = navigator.deviceMemory || 0;
        const maxTouchPoints = navigator.maxTouchPoints || 0;
        const platform = navigator.platform || 'Unknown';

        // Screen orientation (consistent across browsers)
        const screenOrientation = screen.orientation ? screen.orientation.type : 'unknown';

        // Available screen size (excludes taskbar/dock)
        const availWidth = screen.availWidth || screenWidth;
        const availHeight = screen.availHeight || screenHeight;

        // ===========================================
        // HARDWARE ID: Composite of unchanging characteristics
        // ===========================================
        const hardwareComponents = [
            screenWidth,
            screenHeight,
            screenColorDepth,
            screenPixelDepth,
            hardwareConcurrency,
            deviceMemory,
            platform,
            availWidth,
            availHeight
        ].join('|');

        // Hash the hardware components to create Hardware ID
        const hardwareBuffer = await crypto.subtle.digest('SHA-256', encoder.encode(hardwareComponents));
        const hardwareID = Array.from(new Uint8Array(hardwareBuffer))
            .map(b => b.toString(16).padStart(2, '0')).join('');

        console.log('🔧 Hardware ID:', hardwareID.substring(0, 16) + '...');

        // ===========================================
        // BROWSER-SPECIFIC IDENTIFIERS
        // ===========================================
        const userAgent = navigator.userAgent;
        const language = navigator.language || 'Unknown';
        const timezoneOffset = (new Date()).getTimezoneOffset() / -60;
        const timezone = Intl.DateTimeFormat().resolvedOptions().timeZone || 'Unknown';

        // Browser name detection
        const browserName = (function () {
            const ua = userAgent.toLowerCase();
            if (ua.includes('brave')) return 'Brave';
            if (ua.includes('edg/')) return 'Edge';
            if (ua.includes('chrome') && !ua.includes('edge')) return 'Chrome';
            if (ua.includes('firefox')) return 'Firefox';
            if (ua.includes('safari') && !ua.includes('chrome')) return 'Safari';
            if (ua.includes('trident') || ua.includes('msie')) return 'IE';
            if (ua.includes('opera') || ua.includes('opr')) return 'Opera';
            return 'Other';
        })();

        // ===========================================
        // CANVAS FINGERPRINT (Browser-specific but useful)
        // ===========================================
        const canvasFingerprint = await generateCanvasFingerprint();

        // ===========================================
        // WEBGL FINGERPRINT
        // ===========================================
        const webglFingerprint = getWebGLFingerprint();

        // ===========================================
        // AUDIO FINGERPRINT
        // ===========================================
        const audioFingerprint = await generateAudioFingerprint();

        // ===========================================
        // BROWSER FINGERPRINT (for this specific browser instance)
        // ===========================================
        const browserData = [
            userAgent,
            language,
            timezone,
            browserName,
            canvasFingerprint,
            webglFingerprint,
            audioFingerprint
        ].join('|');

        const browserBuffer = await crypto.subtle.digest('SHA-256', encoder.encode(browserData));
        const browserFingerprint = Array.from(new Uint8Array(browserBuffer))
            .map(b => b.toString(16).padStart(2, '0')).join('');

        console.log('🌐 Browser Fingerprint:', browserFingerprint.substring(0, 16) + '...');

        // ===========================================
        // COMBINED FINGERPRINT (for backwards compatibility)
        // ===========================================
        const combinedData = hardwareComponents + '|' + browserData;
        const combinedBuffer = await crypto.subtle.digest('SHA-256', encoder.encode(combinedData));
        const combinedFingerprint = Array.from(new Uint8Array(combinedBuffer))
            .map(b => b.toString(16).padStart(2, '0')).join('');

        // ===========================================
        // POPULATE HIDDEN FIELDS
        // ===========================================

        // Primary identifiers
        const hiddenFingerprint = document.querySelector('[id$="hiddenFingerprint"]');
        const hiddenCanvasFingerprint = document.querySelector('[id$="hiddenCanvasFingerprint"]');

        // NEW: Hardware ID field (you'll need to add this to your ASPX)
        const hiddenHardwareID = document.querySelector('[id$="hiddenHardwareID"]');

        // Device characteristics
        const hiddenOSInfo = document.querySelector('[id$="hiddenOSInfo"]');
        const hiddenBrowserName = document.querySelector('[id$="hiddenBrowserName"]');
        const hiddenScreenResolution = document.querySelector('[id$="hiddenScreenResolution"]');
        const hiddenTimezoneOffset = document.querySelector('[id$="hiddenTimezoneOffset"]');
        const hiddenLanguage = document.querySelector('[id$="hiddenLanguage"]');
        const hiddenHardwareConcurrency = document.querySelector('[id$="hiddenHardwareConcurrency"]');
        const hiddenColorDepth = document.querySelector('[id$="hiddenColorDepth"]');
        const hiddenDeviceMemory = document.querySelector('[id$="hiddenDeviceMemory"]');
        const hiddenMaxTouchPoints = document.querySelector('[id$="hiddenMaxTouchPoints"]');
        const hiddenPlatform = document.querySelector('[id$="hiddenPlatform"]');

        // Set values
        if (hiddenFingerprint) hiddenFingerprint.value = browserFingerprint;
        if (hiddenCanvasFingerprint) hiddenCanvasFingerprint.value = canvasFingerprint;
        if (hiddenHardwareID) hiddenHardwareID.value = hardwareID; // ⭐ NEW

        if (hiddenOSInfo) hiddenOSInfo.value = platform;
        if (hiddenBrowserName) hiddenBrowserName.value = browserName;
        if (hiddenScreenResolution) hiddenScreenResolution.value = `${screenWidth}x${screenHeight}`;
        if (hiddenTimezoneOffset) hiddenTimezoneOffset.value = timezoneOffset.toString();
        if (hiddenLanguage) hiddenLanguage.value = language;
        if (hiddenHardwareConcurrency) hiddenHardwareConcurrency.value = hardwareConcurrency.toString();
        if (hiddenColorDepth) hiddenColorDepth.value = screenColorDepth.toString();
        if (hiddenDeviceMemory) hiddenDeviceMemory.value = deviceMemory.toString();
        if (hiddenMaxTouchPoints) hiddenMaxTouchPoints.value = maxTouchPoints.toString();
        if (hiddenPlatform) hiddenPlatform.value = platform;

        console.log('✓ Enhanced fingerprint generated successfully');
        console.log('Hardware ID will be same across Chrome, Firefox, Brave, Edge, etc.');

    } catch (error) {
        console.error('Fingerprint generation error:', error);

        // Fallback: Use basic hardware info
        const basicHardware = [
            screen.width || 0,
            screen.height || 0,
            screen.colorDepth || 0,
            navigator.hardwareConcurrency || 0,
            navigator.platform || 'unknown'
        ].join('|');

        const encoder = new TextEncoder();
        try {
            const buffer = await crypto.subtle.digest('SHA-256', encoder.encode(basicHardware));
            const fallbackHash = Array.from(new Uint8Array(buffer))
                .map(b => b.toString(16).padStart(2, '0')).join('');

            const hiddenFingerprint = document.querySelector('[id$="hiddenFingerprint"]');
            const hiddenHardwareID = document.querySelector('[id$="hiddenHardwareID"]');

            if (hiddenFingerprint) hiddenFingerprint.value = fallbackHash;
            if (hiddenHardwareID) hiddenHardwareID.value = fallbackHash;

            console.log('✓ Fallback hardware fingerprint generated');
        } catch (e) {
            console.error('Fallback also failed:', e);
        }
    }
}

// ===========================================
// CANVAS FINGERPRINTING
// ===========================================
async function generateCanvasFingerprint() {
    try {
        const canvas = document.createElement('canvas');
        canvas.width = 280;
        canvas.height = 60;
        const ctx = canvas.getContext('2d');

        // Draw diverse content for better uniqueness
        ctx.textBaseline = 'top';
        ctx.font = '14px Arial';
        ctx.fillStyle = '#f60';
        ctx.fillRect(125, 1, 62, 20);

        ctx.fillStyle = '#069';
        ctx.fillText('Device ID 🔒', 2, 15);

        ctx.fillStyle = 'rgba(102, 204, 0, 0.7)';
        ctx.font = '18px "Times New Roman"';
        ctx.fillText('Security Check', 4, 35);

        // Add some shapes
        ctx.beginPath();
        ctx.arc(50, 25, 20, 0, Math.PI * 2, true);
        ctx.closePath();
        ctx.stroke();

        const canvasData = canvas.toDataURL();

        // Hash the canvas data
        const encoder = new TextEncoder();
        const buffer = await crypto.subtle.digest('SHA-256', encoder.encode(canvasData));
        return Array.from(new Uint8Array(buffer))
            .map(b => b.toString(16).padStart(2, '0')).join('');

    } catch (e) {
        console.warn('Canvas fingerprint failed:', e);
        return 'canvas_unavailable';
    }
}

// ===========================================
// WEBGL FINGERPRINTING
// ===========================================
function getWebGLFingerprint() {
    try {
        const canvas = document.createElement('canvas');
        const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');

        if (!gl) return 'webgl_unavailable';

        const debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
        if (!debugInfo) return 'webgl_no_debug';

        const vendor = gl.getParameter(debugInfo.UNMASKED_VENDOR_WEBGL);
        const renderer = gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL);

        return `${vendor}~${renderer}`;
    } catch (e) {
        return 'webgl_error';
    }
}

// ===========================================
// AUDIO FINGERPRINTING
// ===========================================
async function generateAudioFingerprint() {
    try {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        if (!AudioContext) return 'audio_unavailable';

        const context = new AudioContext();
        const oscillator = context.createOscillator();
        const analyser = context.createAnalyser();
        const gainNode = context.createGain();
        const scriptProcessor = context.createScriptProcessor(4096, 1, 1);

        gainNode.gain.value = 0; // Mute
        oscillator.type = 'triangle';
        oscillator.connect(analyser);
        analyser.connect(scriptProcessor);
        scriptProcessor.connect(gainNode);
        gainNode.connect(context.destination);

        oscillator.start(0);

        return new Promise((resolve) => {
            scriptProcessor.onaudioprocess = function (event) {
                const output = event.outputBuffer.getChannelData(0);
                const hash = output.slice(0, 30).reduce((acc, val) => acc + Math.abs(val), 0);

                oscillator.stop();
                scriptProcessor.disconnect();
                gainNode.disconnect();
                analyser.disconnect();
                oscillator.disconnect();
                context.close();

                resolve(hash.toString());
            };

            // Timeout fallback
            setTimeout(() => {
                try {
                    oscillator.stop();
                    context.close();
                } catch (e) { }
                resolve('audio_timeout');
            }, 100);
        });

    } catch (e) {
        return 'audio_error';
    }
}


function detectInstalledFonts() {
    try {
        const baseFonts = ['monospace', 'sans-serif', 'serif'];
        const testFonts = [
            'Arial', 'Verdana', 'Times New Roman', 'Courier New', 'Georgia',
            'Palatino', 'Garamond', 'Bookman', 'Comic Sans MS', 'Trebuchet MS',
            'Impact', 'Lucida Console', 'Tahoma', 'Lucida Sans Unicode',
            'MS Sans Serif', 'MS Serif'
        ];

        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        const testString = 'mmmmmmmmmmlli';
        const fontSize = '72px';

        ctx.textBaseline = 'top';
        ctx.font = fontSize + ' ' + baseFonts[0];

        const baseWidths = {};
        baseFonts.forEach(baseFont => {
            ctx.font = fontSize + ' ' + baseFont;
            baseWidths[baseFont] = ctx.measureText(testString).width;
        });

        const detectedFonts = [];
        testFonts.forEach(font => {
            let detected = false;
            baseFonts.forEach(baseFont => {
                ctx.font = fontSize + ' ' + font + ', ' + baseFont;
                const width = ctx.measureText(testString).width;
                if (width !== baseWidths[baseFont]) {
                    detected = true;
                }
            });
            if (detected) detectedFonts.push(font);
        });

        return detectedFonts.join(',');
    } catch (e) {
        return 'fonts_unavailable';
    }
}


function getPluginsInfo() {
    try {
        if (!navigator.plugins || navigator.plugins.length === 0) {
            return 'no_plugins';
        }

        const plugins = [];
        for (let i = 0; i < navigator.plugins.length; i++) {
            plugins.push(navigator.plugins[i].name);
        }
        return plugins.sort().join(',');
    } catch (e) {
        return 'plugins_error';
    }
}


document.addEventListener('DOMContentLoaded', function () {
    console.log('Initializing enhanced fingerprint system...');
    generateEnhancedFingerprint();
});