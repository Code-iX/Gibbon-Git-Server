const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

(async () => {
    const adminUsername = 'admin';
    const adminPassword = 'admin';

    const browser = await chromium.launch();
    const page = await browser.newPage();

    const takeScreenshot = async (page, url, filename) => {
        const screenshotsDir = path.join(__dirname, 'screenshots');
        if (!fs.existsSync(screenshotsDir)) {
            fs.mkdirSync(screenshotsDir, { recursive: true });
        }
        await page.goto(url, { waitUntil: 'networkidle' });
        await page.screenshot({ path: path.join(screenshotsDir, filename), fullPage: true });
    };

    try {

        const pagesBeforeLogin = [
            { url: 'http://localhost:5000/Login', name: 'login.png' },
            { url: 'http://localhost:5000/ForgotPassword', name: 'forgot_password.png' },
            { url: 'http://localhost:5000/ResetPassword', name: 'reset_password.png' }
        ];

        for (const pageInfo of pagesBeforeLogin) {
            await takeScreenshot(page, pageInfo.url, pageInfo.name);
        }

        await page.goto('http://localhost:5000/Login', { waitUntil: 'networkidle' });

        const antiForgeryToken = await page.getAttribute('input[name="_AntiForgery"]', 'value');
        await page.fill('input[name="Username"]', adminUsername);
        await page.fill('input[name="Password"]', adminPassword);
        await page.evaluate((token) => {
            document.querySelector('input[name="_AntiForgery"]').value = token;
        }, antiForgeryToken);

        await Promise.all([
            page.waitForNavigation({ waitUntil: 'networkidle' }),
            page.click('button[type="submit"]')
        ]);

        if (page.url() === 'http://localhost:5000/Login') {
            throw new Error('Login failed');
        }

        await takeScreenshot(page, 'http://localhost:5000/Repositories/Create', 'create_repository.png');

        await page.fill('input[name="Name"]', 'Gibbon.Git.Server');
        
        await Promise.all([
            page.waitForNavigation({ waitUntil: 'networkidle' }),
            page.click('button[type="submit"]')  
        ]);

        const pagesAfterLogin = [
            { url: 'http://localhost:5000/Repositories', name: 'repositories.png' },
            { url: 'http://localhost:5000/Users', name: 'users.png' },
            { url: 'http://localhost:5000/Teams', name: 'teams.png' },
            { url: 'http://localhost:5000/Account', name: 'account.png' }
        ];

        for (const pageInfo of pagesAfterLogin) {
            await takeScreenshot(page, pageInfo.url, pageInfo.name);
        }

        console.log('Screenshots und Repository erfolgreich erstellt.');
    } catch (error) {
        console.error('Fehler beim Aufnehmen der Screenshots oder beim Erstellen des Repositories:', error);
        process.exit(1);
    } finally {
        await browser.close();
    }
})();
