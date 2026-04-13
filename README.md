# Empath_AI

A comprehensive real-time communication platform powered by AI, built with ASP.NET Core and Gemini API integration. Empath-AI enables emotional intelligence conversations with real-time messaging capabilities, device tracking, and heart rate monitoring.

🚀 Features
Real-time Chat: SignalR-powered bidirectional communication with WebSocket support
AI-Powered Responses: Google Gemini API integration for intelligent conversations
User Authentication: JWT-based authentication with secure token management
Heart Rate Monitoring: Device integration for health metrics tracking
Conversation Management: Persistent conversation history with automatic cleanup
Email Notifications: MailKit integration for email communications
Firebase Integration: Cloud storage and authentication support
RESTful API: Comprehensive API endpoints with Swagger documentation

💻 Tech Stack
Framework: ASP.NET Core 8.0
Database: SQL Server with Entity Framework Core
Real-time Communication: SignalR
Authentication: JWT Bearer Tokens
External APIs: Google Gemini API, Firebase Admin SDK
Security: BCrypt password hashing
Documentation: Swagger/OpenAPI

📦 Dependencies
BCrypt.Net-Next (4.0.3) - Password hashing
Microsoft.AspNetCore.Authentication.JwtBearer - JWT authentication
Microsoft.EntityFrameworkCore & SqlServer - Database ORM
Google.Cloud.Firestore - Cloud database
FirebaseAdmin - Firebase integration
MailKit - Email functionality
Swashbuckle.AspNetCore - Swagger API documentation

🏗️ Project Structure
Code
Empath-AI/
├── Controllers/          # API controllers
├── DTO/                  # Data Transfer Objects
├── Data/                 # Database context and configurations
├── Model/                # Entity models
├── Service/              # Business logic services
├── Repository/           # Data access layer
├── Hubs/                 # SignalR hubs (ChatHub)
├── Migrations/           # EF Core database migrations
├── Properties/           # Project properties
├── Program.cs            # Application entry point
├── appsettings.json      # Configuration file
└── Empath-AI.csproj      # Project file

📧 Email Service (MailKit)
The application uses MailKit (v4.14.1) for email notifications and communications:

Features:
SMTP-based email sending
Support for Gmail, Outlook, and custom SMTP servers
Secure TLS/SSL connections
HTML and plain text email support
Attachment support
Configuration:
The Email service is configured in Program.cs and uses the following settings:

SmtpServer: SMTP server address (e.g., smtp.gmail.com)
SmtpPort: SMTP port (typically 587 for TLS or 465 for SSL)
SenderEmail: Email address for sending notifications
SenderPassword: SMTP password or app-specific password
SenderName: Display name for sent emails

Common Use Cases:
User registration confirmation emails
Password reset notifications
Account verification emails
Email alerts for important events
Conversation summaries

🔐 Authentication
The application uses JWT (JSON Web Tokens) for authentication:
Secure token generation and validation
Support for SignalR real-time connections via query string token
Role-based access control ready

🔌 API Endpoints
Key Features:
User Management: User registration, login, and profile management
Device Management: Register and track user devices
Heart Rate Data: Submit and retrieve heart rate measurements
Conversations: Create, retrieve, and manage conversations
Messages: Send and receive messages with AI responses
SignalR Hub
ChatHub (/hubs/Chat): Real-time messaging with JWT authentication support

🚀 Getting Started
Prerequisites
.NET 8.0 SDK
SQL Server
Google Gemini API Key
Firebase Project (optional)

📱 CORS Configuration
Currently configured to allow requests from:
http://localhost:5280
http://127.0.0.1:5280
Update the CORS policy in Program.cs for production deployment.

🔄 Services
GeminiService: Handles AI API interactions
EmailService: Manages email communications
TokenService: JWT token generation and validation
ConversationCleanupService: Scheduled service for conversation maintenance

📊 Database Models
User: User account information
Device: User devices for health tracking
HeartRate: Heart rate data points
Conversation: Chat conversation sessions
Message: Individual messages within conversations


📝 License
This project is private. Contact the repository owner for licensing information.

👤 Author
Ahmedezz283

🤝 Contributing
For contributions, please contact the repository owner.

📞 Support
For issues and questions, please open an issue on the GitHub repository.
