const { chromium } = require('playwright');

(async () => {
    const browser = await chromium.launch();
    const page = await browser.newPage();

    await page.goto('http://localhost:5000/Login');
        await page.screenshot({ path: pageInfo.name, fullPage: true });

    await page.fill('input[name="Username"]', 'admin');
    await page.fill('input[name="Password"]', 'admin');

    await Promise.all([
        page.waitForNavigation(),
        page.click('button[type="submit"]')
    ]);

    if (page.url() !== 'http://localhost:5000/') {
        console.error('Login failed');
        await browser.close();
        process.exit(1);
    }

    const pagesToScreenshot = [
        { url: 'http://localhost:5000/Dashboard', name: 'dashboard.png' },
        { url: 'http://localhost:5000/Profile', name: 'profile.png' },
    ];

    for (const pageInfo of pagesToScreenshot) {
        await page.goto(pageInfo.url);
        await page.screenshot({ path: pageInfo.name, fullPage: true });
    }

    await browser.close();
})();
