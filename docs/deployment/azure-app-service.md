# Deploying Endatix to Azure App Service

This guide provides step-by-step instructions for deploying Endatix Platform to Azure App Service with PostgreSQL.

## Prerequisites

- Azure CLI installed and configured (`az login`)
- .NET 9.0 SDK installed
- Access to an Azure subscription
- Basic understanding of Azure App Service and PostgreSQL

## Architecture Overview

The deployment consists of:
- **Azure App Service** (Linux, .NET 9.0) - Hosts the Endatix API
- **Azure Database for PostgreSQL Flexible Server** - Database backend
- **App Service Plan** (Basic B1 or higher) - Compute resources

## Deployment Steps

### 1. Authenticate with Azure

```bash
az login
az account show
```

### 2. Create Resource Group

```bash
# Set variables
RESOURCE_GROUP="cortex_ai_forms_v2"
LOCATION="eastus2"
APP_NAME="caif-backend"
DB_NAME="caif-backend-db"
DB_ADMIN_USER="caifadmin"
DB_ADMIN_PASSWORD="<generate-secure-password>"

# Create resource group (if it doesn't exist)
az group create --name $RESOURCE_GROUP --location $LOCATION
```

### 3. Create App Service Plan

```bash
az appservice plan create \
  --name ${APP_NAME}-plan \
  --resource-group $RESOURCE_GROUP \
  --sku B1 \
  --is-linux
```

### 4. Create PostgreSQL Flexible Server

```bash
# Generate a secure password
DB_ADMIN_PASSWORD=$(openssl rand -base64 24 | tr -d '=+/' | cut -c1-20)
echo "Database password: $DB_ADMIN_PASSWORD"  # Save this!

# Create PostgreSQL server
az postgres flexible-server create \
  --name $DB_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $DB_ADMIN_USER \
  --admin-password "$DB_ADMIN_PASSWORD" \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --storage-size 32 \
  --version 16 \
  --public-access 0.0.0.0-255.255.255.255 \
  --yes
```

**Note:** The `--public-access` parameter allows connections from Azure services. For production, configure firewall rules appropriately.

### 5. Create Database

```bash
az postgres flexible-server db create \
  --resource-group $RESOURCE_GROUP \
  --server-name $DB_NAME \
  --database-name endatix
```

### 6. Create Web App

```bash
az webapp create \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan ${APP_NAME}-plan \
  --runtime "DOTNETCORE:9.0"
```

### 7. Configure Application Settings

```bash
# Generate JWT signing key
JWT_KEY=$(openssl rand -base64 32)

# Configure environment variables
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    ASPNETCORE_ENVIRONMENT="Production" \
    ConnectionStrings__DefaultConnection="Host=${DB_NAME}.postgres.database.azure.com;Database=endatix;Username=${DB_ADMIN_USER};Password=${DB_ADMIN_PASSWORD};SslMode=Require" \
    ConnectionStrings__DefaultConnection__DbProvider="PostgreSql" \
    Endatix__Persistence__DatabaseProvider="PostgreSql" \
    Endatix__Auth__Providers__EndatixJwt__SigningKey="$JWT_KEY" \
    Endatix__Data__EnableAutoMigrations="true" \
    Endatix__Data__InitialUser__Email="admin@endatix.com" \
    Endatix__Data__InitialUser__Password="P@ssw0rd" \
    Endatix__Data__SeedSampleData="false"
```

**Security Note:** Change the default admin password after first login.

### 8. Build and Deploy Application

```bash
# From the repository root
dotnet publish src/Endatix.WebHost/Endatix.WebHost.csproj -c Release -o ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy to Azure
az webapp deploy \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --src-path deploy.zip \
  --type zip \
  --async true
```

### 9. Enable Application Logging

```bash
az webapp log config \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --application-logging filesystem \
  --level verbose \
  --docker-container-logging filesystem
```

### 10. Verify Deployment

```bash
# Check app status
az webapp show \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --query "{name:name,state:state,defaultHostName:defaultHostName}" \
  --output table

# Test the API
curl https://${APP_NAME}.azurewebsites.net/health
```

## Post-Deployment Configuration

### Access Your Application

- **API Base URL:** `https://caif-backend.azurewebsites.net/api/`
- **Swagger Documentation:** `https://caif-backend.azurewebsites.net/api-docs`
- **Health Check:** `https://caif-backend.azurewebsites.net/health`

### Initial User Setup

The application automatically creates an initial admin user on first startup:

- **Email:** `admin@endatix.com`
- **Password:** `P@ssw0rd` (or as configured in `Endatix__Data__InitialUser__Password`)

**Important:** The initial user is created with `EmailConfirmed = true` and assigned to `TenantId = 1` (Default Tenant).

### Common Post-Deployment Issues

#### Issue 1: User Cannot Login - "User not found"

This occurs when:
- The initial user seeding didn't run
- Email is not confirmed in the database
- User has incorrect tenant assignment

**Solution:**
```bash
# Connect to PostgreSQL and fix the user
az postgres flexible-server execute \
  --name $DB_NAME \
  --admin-user $DB_ADMIN_USER \
  --admin-password "$DB_ADMIN_PASSWORD" \
  --database-name endatix \
  --querytext "UPDATE identity.\"AspNetUsers\" SET \"EmailConfirmed\" = true, \"TenantId\" = 1 WHERE \"Email\" = 'admin@endatix.com';"
```

#### Issue 2: Database Tables Not Created

Migrations should run automatically if `EnableAutoMigrations` is true. If not:

```bash
# Check if migrations ran
az postgres flexible-server execute \
  --name $DB_NAME \
  --admin-user $DB_ADMIN_USER \
  --admin-password "$DB_ADMIN_PASSWORD" \
  --database-name endatix \
  --querytext "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' OR table_schema = 'identity';"

# Restart the app to trigger migrations
az webapp restart --name $APP_NAME --resource-group $RESOURCE_GROUP
```

#### Issue 3: 404 Errors on API Endpoints

Ensure you're using the correct base path `/api/` prefix:

- ✅ Correct: `https://caif-backend.azurewebsites.net/api/auth/login`
- ❌ Wrong: `https://caif-backend.azurewebsites.net/auth/login`

### Configure CORS (Optional)

If you need to allow requests from a frontend application:

```bash
az webapp cors add \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --allowed-origins "https://your-frontend-domain.com"
```

Or update `appsettings.json` or environment variables:

```bash
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    Endatix__Cors__CorsPolicies__0__AllowedOrigins__0="https://your-frontend-domain.com" \
    Endatix__Cors__CorsPolicies__0__AllowedOrigins__1="https://another-domain.com"
```

## Monitoring and Troubleshooting

### View Application Logs

```bash
# Stream logs in real-time
az webapp log tail \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP

# Download log files
az webapp log download \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --log-file webapp-logs.zip
```

### Check Database Connection

```bash
# Test database connectivity
az postgres flexible-server execute \
  --name $DB_NAME \
  --admin-user $DB_ADMIN_USER \
  --admin-password "$DB_ADMIN_PASSWORD" \
  --database-name endatix \
  --querytext "SELECT version();"
```

### View Environment Variables

```bash
az webapp config appsettings list \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --output table
```

## Scaling and Performance

### Scale Up (Vertical Scaling)

```bash
# Upgrade to S1 for production
az appservice plan update \
  --name ${APP_NAME}-plan \
  --resource-group $RESOURCE_GROUP \
  --sku S1
```

### Scale Out (Horizontal Scaling)

```bash
# Add more instances
az appservice plan update \
  --name ${APP_NAME}-plan \
  --resource-group $RESOURCE_GROUP \
  --number-of-workers 3
```

### Database Scaling

```bash
# Upgrade database tier
az postgres flexible-server update \
  --name $DB_NAME \
  --resource-group $RESOURCE_GROUP \
  --sku-name Standard_D2s_v3 \
  --tier GeneralPurpose
```

## Security Best Practices

1. **Change Default Passwords:**
   - Update the default admin password immediately after deployment
   - Rotate database passwords regularly

2. **Restrict Database Access:**
   - Configure firewall rules to only allow Azure services
   - Use private endpoints for production deployments

3. **Use Managed Identities:**
   - Consider using Azure Managed Identity instead of connection strings
   - Implement Key Vault for sensitive configuration

4. **Enable HTTPS Only:**
   ```bash
   az webapp update \
     --name $APP_NAME \
     --resource-group $RESOURCE_GROUP \
     --https-only true
   ```

5. **Configure Custom Domain:**
   ```bash
   az webapp config hostname add \
     --webapp-name $APP_NAME \
     --resource-group $RESOURCE_GROUP \
     --hostname api.yourdomain.com
   ```

## Backup and Recovery

### Database Backups

Azure PostgreSQL Flexible Server provides automated backups. Configure retention:

```bash
az postgres flexible-server update \
  --name $DB_NAME \
  --resource-group $RESOURCE_GROUP \
  --backup-retention 30
```

### Application Backups

```bash
# Create a backup
az webapp config backup create \
  --resource-group $RESOURCE_GROUP \
  --webapp-name $APP_NAME \
  --backup-name manual-backup-$(date +%Y%m%d) \
  --container-url "<storage-account-sas-url>"
```

## Clean Up Resources

To remove all resources:

```bash
az group delete \
  --name $RESOURCE_GROUP \
  --yes \
  --no-wait
```

## Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [Azure Database for PostgreSQL Documentation](https://docs.microsoft.com/azure/postgresql/)
- [Endatix Documentation](https://docs.endatix.com/)
- [Azure CLI Reference](https://docs.microsoft.com/cli/azure/)

## Current Deployment Information

### Production Deployment Details

- **Resource Group:** `cortex_ai_forms_v2`
- **App Service:** `caif-backend`
- **Database Server:** `caif-backend-db`
- **Database Name:** `endatix`
- **Location:** `East US 2`
- **URL:** `https://caif-backend.azurewebsites.net`
- **API Base:** `https://caif-backend.azurewebsites.net/api/`
- **Swagger:** `https://caif-backend.azurewebsites.net/api-docs`

### Database Schema

The deployment uses two schemas:
- **`public` schema:** Application data (Forms, Submissions, Tenants, etc.)
- **`identity` schema:** Identity tables (AspNetUsers, AspNetRoles, etc.)

### Current Tenant Configuration

- **Default Tenant ID:** 1
- **Default Tenant Name:** "Default Tenant"
- **Admin User Email:** admin@endatix.com
- **Admin User Tenant:** Tenant #1

All forms, submissions, and data are scoped to Tenant #1 by default.
