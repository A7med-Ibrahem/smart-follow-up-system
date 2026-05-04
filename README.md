# 🏥 Smart Follow Up

<div align="center">

![Smart Follow Up Banner](https://img.shields.io/badge/Smart%20Follow%20Up-AI%20Powered%20Healthcare-00d4ff?style=for-the-badge&logo=heart&logoColor=white)

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Express-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://docs.microsoft.com/ef/)
[![JWT](https://img.shields.io/badge/JWT-Authentication-000000?style=flat-square&logo=jsonwebtokens&logoColor=white)](https://jwt.io/)
[![Swagger](https://img.shields.io/badge/Swagger-API%20Docs-85EA2D?style=flat-square&logo=swagger&logoColor=black)](https://swagger.io/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

**AI-powered post-operative patient monitoring system**

[Features](#-features) • [Tech Stack](#-tech-stack) • [Getting Started](#-getting-started) • [API Docs](#-api-documentation) • [Team](#-team)

</div>

---

## 📋 Overview

**Smart Follow Up** is a comprehensive healthcare platform designed for post-operative patient monitoring. It enables doctors to create and manage patient cases, track daily health reports, receive AI-powered risk alerts, and communicate treatment plans — all in one place.

> Built with ASP.NET Core Web API + Entity Framework Core + SQL Server

---

## ✨ Features

### 👨‍⚕️ Doctor
- Create and manage patient cases after surgery
- Review daily patient health reports
- AI-powered risk level classification (Stable / Moderate / Critical)
- Receive instant critical alerts
- Manage prescriptions and medications
- Add medical notes to patient profiles
- View wound healing image timeline
- Search patients by name or phone

### 🧑‍🦱 Patient
- Submit daily health reports (temperature, pain, swelling, bleeding)
- Upload wound images
- View prescriptions and medication instructions
- Confirm medication intake
- Receive smart notifications and reminders
- View personal risk status

### 👑 Admin
- Review and approve/reject doctor registration requests
- Manage doctor accounts (activate/deactivate)
- View platform analytics and statistics

### 🤖 AI System
- Automatic risk classification after each report submission
- Critical condition alert generation
- Emergency escalation via background jobs (Hangfire)
- Wound image validation

---

## 🛠 Tech Stack

### Backend
| Technology | Version | Purpose |
|---|---|---|
| ASP.NET Core Web API | .NET 10 | REST API Framework |
| Entity Framework Core | Latest | ORM & Database Management |
| SQL Server Express | 2022 | Database |
| JWT Bearer | Latest | Authentication & Authorization |
| BCrypt.Net | Latest | Password Hashing |
| MailKit | Latest | Email Service |
| Hangfire | Latest | Background Jobs |
| Swashbuckle | 6.5 | API Documentation |

### Frontend
| Technology | Purpose |
|---|---|
| HTML5 + CSS3 | Structure & Styling |
| Vanilla JavaScript | Interactivity & API Integration |
| Space Grotesk + Inter | Typography |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Installation

**1. Clone the repository**
```bash
git clone https://github.com/your-username/smart-follow-up.git
cd smart-follow-up
```

**2. Setup User Secrets (Development)**
```bash
cd smartFollowup.API

dotnet user-secrets set "Jwt:Key" "YourSecretKeyHere"
dotnet user-secrets set "Jwt:Issuer" "SmartFollowUpAPI"
dotnet user-secrets set "Jwt:Audience" "SmartFollowUpClient"
dotnet user-secrets set "Jwt:ExpireDays" "7"
dotnet user-secrets set "Email:Host" "smtp.gmail.com"
dotnet user-secrets set "Email:Port" "587"
dotnet user-secrets set "Email:Username" "your-email@gmail.com"
dotnet user-secrets set "Email:Password" "your-app-password"
dotnet user-secrets set "Email:FromName" "Smart Follow Up"
```

**3. Update Connection String in `appsettings.json`**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=SmartFollowUpDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**4. Run Database Migrations**
```bash
dotnet ef database update
```

**5. Run the Application**
```bash
dotnet run
```

**6. Open Swagger UI**
```
http://localhost:5282/swagger
```

---

## 📡 API Documentation

### Authentication
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/Auth/login` | Login for all users | ❌ |
| POST | `/api/Auth/register-doctor` | Register new doctor | ❌ |
| POST | `/api/Auth/register-patient` | Register new patient | ❌ |
| POST | `/api/Auth/request-doctor` | Submit doctor access request | ❌ |
| POST | `/api/Auth/forgot-password` | Request password reset token | ❌ |
| POST | `/api/Auth/reset-password` | Reset password with token | ❌ |

### Cases
| Method | Endpoint | Description | Role |
|---|---|---|---|
| POST | `/api/Cases` | Create new patient case | Doctor |
| GET | `/api/Cases` | Get all doctor's cases | Doctor |
| GET | `/api/Cases/{id}` | Get case by ID | Doctor |
| GET | `/api/Cases/search?keyword=` | Search patients | Doctor |
| PUT | `/api/Cases/{id}/close` | Close a case | Doctor |

### Daily Reports
| Method | Endpoint | Description | Role |
|---|---|---|---|
| POST | `/api/Reports` | Submit daily report | Patient |
| GET | `/api/Reports/case/{caseId}` | Get case reports | Doctor |

### Prescriptions
| Method | Endpoint | Description | Role |
|---|---|---|---|
| POST | `/api/Prescriptions` | Create prescription | Doctor |
| PUT | `/api/Prescriptions/{id}` | Update prescription | Doctor |
| GET | `/api/Prescriptions/case/{caseId}` | Get case prescriptions | Doctor/Patient |
| POST | `/api/Prescriptions/medications/{id}/confirm` | Confirm medication taken | Patient |

### Alerts & Notifications
| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | `/api/Alerts` | Get all alerts | Doctor |
| PUT | `/api/Alerts/{id}/handle` | Handle alert | Doctor |
| GET | `/api/Alerts/count` | Get open alerts count | Doctor |
| GET | `/api/Notifications` | Get notifications | All |
| PUT | `/api/Notifications/{id}/read` | Mark as read | All |
| PUT | `/api/Notifications/read-all` | Mark all as read | All |
| GET | `/api/Notifications/count` | Get unread count | All |

### Patient
| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | `/api/Patient/profile` | Get patient profile | Patient |
| PUT | `/api/Patient/profile` | Update patient profile | Patient |
| GET | `/api/Patient/risk-status` | Get current risk level | Patient |

### Wound Images
| Method | Endpoint | Description | Role |
|---|---|---|---|
| POST | `/api/WoundImages/upload/{reportId}` | Upload wound image | Patient |
| GET | `/api/WoundImages/case/{caseId}` | Get case images timeline | Doctor/Patient |

### Admin
| Method | Endpoint | Description | Role |
|---|---|---|---|
| GET | `/api/Admin/analytics` | Get platform analytics | Admin |
| GET | `/api/Admin/doctor-requests` | Get doctor requests | Admin |
| PUT | `/api/Admin/doctor-requests/{id}/approve` | Approve doctor | Admin |
| PUT | `/api/Admin/doctor-requests/{id}/reject` | Reject doctor | Admin |
| PUT | `/api/Admin/doctors/{id}/toggle-status` | Toggle doctor status | Admin |

---

## 🗄 Database Schema

```
Users ──────────────── DoctorProfiles
  │                    PatientProfiles
  │
  ├── Cases ─────────── DailyReports ── AiAnalyses
  │     │                    │
  │     │                    └── WoundImages
  │     │
  │     ├── Prescriptions ── PrescriptionMedications ── MedicationAdherences
  │     ├── DoctorNotes
  │     └── Alerts ──────── Notifications
  │
  └── DoctorRequests
```

---

## 🔐 Security

- **JWT Authentication** — All protected endpoints require Bearer token
- **BCrypt Password Hashing** — Passwords never stored in plain text
- **User Secrets** — Sensitive credentials stored outside codebase
- **CORS Policy** — Configured for development and production
- **Role-Based Authorization** — Doctor / Patient / Admin roles enforced

---

## 📁 Project Structure

```
SmartFollowUp/
├── smartFollowup.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── CasesController.cs
│   │   ├── ReportsController.cs
│   │   ├── PrescriptionsController.cs
│   │   ├── NotesController.cs
│   │   ├── AlertsController.cs
│   │   ├── NotificationsController.cs
│   │   ├── WoundImagesController.cs
│   │   ├── PatientController.cs
│   │   └── AdminController.cs
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Case.cs
│   │   ├── DailyReport.cs
│   │   ├── AiAnalysis.cs
│   │   ├── Prescription.cs
│   │   ├── PrescriptionMedication.cs
│   │   ├── MedicationAdherence.cs
│   │   ├── DoctorNote.cs
│   │   ├── Alert.cs
│   │   ├── Notification.cs
│   │   ├── WoundImage.cs
│   │   ├── DoctorProfile.cs
│   │   ├── PatientProfile.cs
│   │   └── DoctorRequest.cs
│   ├── DTOs/
│   │   ├── AuthDTOs.cs
│   │   ├── CaseDTOs.cs
│   │   ├── ReportDTOs.cs
│   │   ├── PrescriptionDTOs.cs
│   │   ├── NoteDTOs.cs
│   │   ├── AlertDTOs.cs
│   │   ├── NotificationDTOs.cs
│   │   ├── WoundImageDTOs.cs
│   │   ├── PatientDTOs.cs
│   │   └── AdminDTOs.cs
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── CaseService.cs
│   │   ├── ReportService.cs
│   │   ├── PrescriptionService.cs
│   │   ├── NoteService.cs
│   │   ├── AlertService.cs
│   │   ├── NotificationService.cs
│   │   ├── WoundImageService.cs
│   │   ├── PatientService.cs
│   │   ├── AdminService.cs
│   │   ├── EmailService.cs
│   │   └── EscalationService.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── appsettings.json
│   └── Program.cs
│
└── SmartFollowUp-Frontend/
    ├── auth/
    │   ├── login.html
    │   ├── forgot-password.html
    │   └── doctor-request.html
    ├── doctor/
    │   ├── doctor-dashboard.html
    │   └── patient-profile.html
    ├── patient/
    │   └── patient-dashboard.html
    ├── admin/
    │   └── admin-dashboard.html
    └── shared/
        ├── css/styles.css
        └── js/
            ├── api.js
            └── auth.js
```

---

## 👥 Team

| Role | Responsibility |
|---|---|
| Backend Lead | API, Database, Authentication |
| Frontend Dev 1 | Auth Pages |
| Frontend Dev 2 | Doctor Dashboard & Patient Profile |
| Frontend Dev 3 | Patient Dashboard |
| Frontend Dev 4 | Admin Dashboard |

---

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

---

<div align="center">

Made with ❤️ by the Smart Follow Up Team

</div>
