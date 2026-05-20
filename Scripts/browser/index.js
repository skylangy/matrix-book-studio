const { app, BrowserWindow, session } = require('electron');
const fs = require('fs');
const path = require('path');

const CONFIG_FILE = path.join(__dirname, 'config.json');

let windows = [];

app.whenReady().then(() => {
    // Load configurations
    const configs = loadConfigurations(CONFIG_FILE);

    if (!configs || configs.length === 0) {
        console.error('No configurations found. Exiting...');
        app.quit();
        return;
    }

    // Create a window for each configuration
    configs.forEach((config, index) => {

        const customSession = session.fromPartition(`persist:${config.session}`);

        const win = new BrowserWindow({
            width: 1920,
            height: 1200,
            title: config.name,
            webPreferences: {
                session: customSession, // Assign unique session
                contextIsolation: true,
                nodeIntegration: false,
            },
        });

        win.setMenuBarVisibility(false);
        win.loadURL(config.url);

        win.on('closed', () => {
            windows = windows.filter(w => w !== win);
        });

        win.webContents.on('page-title-updated', (event, title) => {
            event.preventDefault();
            win.setTitle(`${config.name} - ${title}`);
        });

        win.webContents.on('before-input-event', (event, input) => {
            if (input.key === 'F5' && input.type === 'keyDown') {
                win.reload();
                event.preventDefault();
            }
        });

        windows.push(win);
    });
});

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') {
        app.quit();
    }
});

// Function to load configuration from a file
function loadConfigurations(filePath) {
    try {
        const data = fs.readFileSync(filePath, 'utf8');
        return JSON.parse(data);
    } catch (err) {
        console.error('Failed to load configurations:', err);
        return null;
    }
}
