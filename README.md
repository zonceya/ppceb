# PPECB Assessment - Product & Category Management System

## Architecture Diagram

![Architecture Diagram](docs/architecture-diagram.png)

*If the image doesn't load, view `docs/architecture-diagram.txt` for the text version.*

## Overview
A secure, scalable web application that allows users to manage categories and products. Each user can only access their own data.

## Features

### Core Features
- ✅ User Registration & Login (Email/Password)
- ✅ Category Management (Create, Read, Update)
- ✅ Product Management (Create, Read, Update, Delete)
- ✅ Product Image Upload
- ✅ Excel Import/Export for Products
- ✅ Auto-generated Product Codes (format: yyyyMM-###)
- ✅ Category Code Validation (format: ABC123)
- ✅ Pagination for Products (10 per page)

### Technical Features
- ✅ RESTful API with JWT Authentication
- ✅ Swagger/OpenAPI Documentation
- ✅ N-Tier Architecture
- ✅ SOLID Principles
- ✅ Audit Fields (CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
- ✅ User Data Isolation

## Prerequisites

| Requirement | Version |
|-------------|---------|
| Visual Studio | 2022 or later |
| .NET SDK | 8.0 (LTS) |
| SQL Server | LocalDB 2022+ (comes with Visual Studio) |

## Quick Start

### 1. Clone the Repository
```bash
git clone https://github.com/zonceya/PPECB-Assessment.git
cd PPECB-Assessment