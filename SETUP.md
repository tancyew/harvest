# Harvest Platform - Setup Guide

## New Features Implemented

### 1. Authentication System
- **OTP-based login** with dummy mode for testing
- **Session management** using ProtectedSessionStorage
- **Mobile-friendly login/register pages**
- **User roles**: Customer, Admin

### 2. E-Commerce Shopping Interface
- **Mobile-optimized shop page** for customers
- **Product browsing** with search and category filters
- **Shopping cart** with add/remove/update functionality
- **Customer-specific pricing** support
- **Real-time stock availability** display

### 3. PDF Generation with Gotenberg
- **Invoice PDF generation** from HTML templates
- **Sales Order PDF generation**
- **Print-friendly pages** for invoices and orders
- **Download PDF** functionality

## Quick Start

### Prerequisites

1. **.NET 9 SDK** installed
2. **Docker** installed (for Gotenberg PDF service)
3. **SQLite** (included with .NET)

### Step 1: Start Gotenberg Service

Gotenberg is a Docker-based service that converts HTML to PDF.

```bash
# Pull and run Gotenberg Docker container
docker run -d --rm -p 3000:3000 gotenberg/gotenberg:8
```

Verify Gotenberg is running:
```bash
curl http://localhost:3000/health
```

### Step 2: Run the Application

```bash
# Navigate to project directory
cd harvest

# Restore dependencies
dotnet restore

# Run the application
dotnet run
```

The application will be available at:
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000

### Step 3: Test the Features

## Test User Accounts

The following test accounts are pre-seeded in the database:

| User | Email/Phone | OTP Code | Role |
|------|-------------|----------|------|
| Admin | admin@harvest.com | 123456 | Admin |
| Admin | +60123456789 | 123456 | Admin |
| John Tan | john@customer.com | 123456 | Customer |
| John Tan | +60123456001 | 123456 | Customer |
| Mary Wong | mary@customer.com | 123456 | Customer |
| Mary Wong | +60123456002 | 123456 | Customer |

**Note**: In dummy mode, OTP code is always `123456` for all users.

## Features Walkthrough

### 1. Customer E-Commerce Flow

1. **Browse Products**
   - Go to `/shop` or click "Shop" in menu
   - You'll be redirected to login if not authenticated

2. **Login**
   - Go to `/login`
   - Enter email or phone: `john@customer.com` or `+60123456001`
   - Click "Request OTP"
   - Enter OTP: `123456`
   - Click "Verify & Login"

3. **Shop for Products**
   - Browse products in the catalog
   - Use search bar to find products
   - Filter by category (Vegetables, Fruits)
   - View customer-specific pricing (if available)
   - Add products to cart

4. **Cart & Checkout**
   - Click cart icon to view shopping cart
   - Adjust quantities with +/- buttons
   - Remove items if needed
   - Click "Place Order" to checkout

5. **Register New Customer**
   - Go to `/register`
   - Fill in business and contact details
   - Click "Create Account"
   - Login with your credentials

### 2. PDF Generation

1. **Generate Invoice PDF**
   - Go to any invoice details page (e.g., `/invoices/1`)
   - Click "Download PDF" button
   - PDF will be generated via Gotenberg and downloaded
   - Or click "Print Invoice" to view print-friendly page

2. **Generate Order PDF**
   - Similar to invoice, navigate to `/print/order/{orderId}`

### 3. Admin Functions

1. **Login as Admin**
   - Email: `admin@harvest.com`
   - OTP: `123456`

2. **Manage Products**
   - Go to `/products`
   - Add/Edit/Delete products
   - Upload product images (directory created at `wwwroot/images/products/`)

3. **Manage Customers**
   - Go to `/customers`
   - View all customer accounts
   - Check customer pricing

4. **Manage Orders & Invoices**
   - Go to `/orders` or `/invoices`
   - View all orders and invoices
   - Generate PDFs

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=harvest.db"
  },
  "OtpSettings": {
    "DummyMode": true,          // Set to false for production
    "DummyOtpCode": "123456",   // Dummy OTP code
    "OtpExpiryMinutes": 5       // OTP expiry time
  },
  "GotenbergSettings": {
    "Url": "http://localhost:3000"  // Gotenberg service URL
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@harvest.com",
    "SenderName": "Harvest Platform",
    "EnableSsl": true
  }
}
```

### Production Deployment

For production deployment:

1. **Disable Dummy OTP Mode**
   ```json
   "OtpSettings": {
     "DummyMode": false,
     ...
   }
   ```

2. **Implement Real OTP Delivery**
   - Integrate SMS service (Twilio, AWS SNS, etc.)
   - Integrate email service (SendGrid, Mailgun, etc.)
   - Update `AuthService.RequestOtpAsync()` method

3. **Deploy Gotenberg**
   - Run Gotenberg as a separate service
   - Use Docker Compose or Kubernetes
   - Update `GotenbergSettings:Url` in appsettings

4. **Use Production Database**
   - Switch from SQLite to PostgreSQL/MySQL/SQL Server
   - Update connection string
   - Run proper EF migrations

## File Structure (New Files)

```
Harvest/
├── Components/
│   ├── Pages/
│   │   ├── Login.razor              # Mobile-friendly login page
│   │   ├── Register.razor           # Customer registration
│   │   ├── Shop.razor               # E-commerce shopping interface
│   │   ├── InvoicePrint.razor       # Invoice print template
│   │   └── OrderPrint.razor         # Order print template
├── Services/
│   ├── AuthService.cs               # OTP authentication service
│   ├── SessionService.cs            # User session management
│   └── GotenbergService.cs          # PDF generation service
├── wwwroot/
│   ├── images/
│   │   ├── products/               # Product images directory
│   │   └── users/                  # User images directory
│   └── js/
│       └── download.js             # File download helper
├── appsettings.json                # Updated with OTP & Gotenberg config
└── SETUP.md                        # This file
```

## Mobile Optimization

All customer-facing pages are optimized for mobile devices:
- **Login/Register**: Responsive forms with large touch targets
- **Shop**: Grid layout adapts to screen size
- **Product Cards**: Mobile-friendly with clear pricing
- **Cart**: Drawer-based cart for easy mobile access

## Troubleshooting

### PDF Generation Fails

**Error**: "Failed to generate PDF. Make sure Gotenberg service is running."

**Solution**:
1. Check if Gotenberg is running: `docker ps`
2. Verify Gotenberg health: `curl http://localhost:3000/health`
3. Restart Gotenberg: `docker restart <container-id>`

### Login Not Working

**Error**: "User not found" or "Invalid OTP"

**Solution**:
1. Verify user exists in database
2. Check OTP code is `123456` (dummy mode)
3. Ensure `DummyMode` is `true` in appsettings.json
4. Check database was seeded: Delete `harvest.db` and restart app

### Images Not Showing

**Error**: Product images show placeholder

**Solution**:
1. Verify directories exist: `ls -la wwwroot/images/`
2. Upload images to `wwwroot/images/products/`
3. Update product records with image paths

### Session Lost on Page Refresh

**Issue**: User logged out after page refresh

**Solution**:
- This is expected in Blazor Server during development
- Session storage is per-circuit
- For production, consider using cookies or JWT tokens

## Next Steps

1. **Implement Real OTP** - Integrate SMS/Email services
2. **Payment Gateway** - Add payment processing
3. **Email Notifications** - Send order confirmations
4. **Image Upload** - Implement product image upload UI
5. **Order Tracking** - Add order status updates
6. **Reports & Analytics** - Sales reports and dashboards

## Support

For issues or questions:
- Check logs: Application logs show detailed error messages
- Database: Use SQLite browser to inspect data
- Gotenberg: Check Docker logs: `docker logs <container-id>`

---

**Harvest Platform** - Fresh Produce Direct from Farm to Customer
