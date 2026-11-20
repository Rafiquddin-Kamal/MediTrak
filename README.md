# MediTrack – ASP.NET Core MVC + MongoDB (Login module)

## Stack
- **Backend:** ASP.NET Core MVC (.NET 10), C#
- **Frontend:** Razor views + Bootstrap 5.3 + vanilla JavaScript
- **Database:** MongoDB (official `MongoDB.Driver`)
- **Auth:** Cookie authentication, passwords hashed with ASP.NET Core `PasswordHasher` (PBKDF2)

## Folder structure and MVC mapping

```
MediTrack.Web/
├── Controllers/            ← C: handle requests, validate input, choose the view
│   ├── AccountController.cs    (Login GET/POST, Logout)
│   └── HomeController.cs       (protected dashboard, error page)
├── Models/                 ← M: data shapes
│   ├── User.cs                 (MongoDB document in the "users" collection)
│   ├── Role.cs
│   └── ViewModels/
│       └── LoginViewModel.cs   (what the login form posts)
├── Views/                  ← V: Razor + Bootstrap, no business logic
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   ├── Account/Login.cshtml
│   ├── Home/Index.cshtml
│   └── Shared/_Layout.cshtml, Error.cshtml
├── Services/               ← M (business logic): login rules, lockout, hashing
│   ├── IAuthService.cs
│   └── AuthService.cs
├── Repositories/           ← M (data access): every MongoDB query
│   ├── IUserRepository.cs
│   └── UserRepository.cs
├── Data/
│   ├── MongoDbContext.cs       (MongoClient + typed collections)
│   └── DbSeeder.cs             (unique email index + dev demo user)
├── Settings/MongoDbSettings.cs
├── wwwroot/                ← static files served to the browser
│   ├── css/site.css
│   └── js/login.js             (show/hide password, client validation)
├── Properties/launchSettings.json
├── appsettings.json            (MongoDB connection string)
├── appsettings.Development.json (demo user)
├── docker-compose.yml          (optional MongoDB in Docker)
├── Program.cs              ← dependency injection, auth, routing
└── MediTrack.Web.csproj
```

Request flow: `Login.cshtml` → `AccountController.Login` → `AuthService.ValidateCredentialsAsync`
→ `UserRepository.GetByEmailAsync` → MongoDB → back up the chain → cookie issued → redirect to `Home/Index`.

## Required software and packages
| Item | Purpose |
|---|---|
| .NET 10 SDK | Build and run the app |
| MongoDB Community Server 8 (or Docker, or MongoDB Atlas) | Database |
| `mongosh` | Check data from the terminal (installed with `mongodb-org`) |
| NuGet: `MongoDB.Driver` | MongoDB access from C# (restored automatically) |
| Bootstrap 5.3 (CDN) | Layout and form styling, nothing to install |

## Run it on Ubuntu (WSL)

**1. Install .NET 10**
```bash
sudo add-apt-repository ppa:dotnet/backports     # Ubuntu 22.04 / 24.04 only
sudo apt update && sudo apt install -y dotnet-sdk-10.0
dotnet --list-sdks
```

**2. Install MongoDB 8** (not in Ubuntu's own repositories, so add MongoDB's)
```bash
sudo apt install -y gnupg curl
curl -fsSL https://www.mongodb.org/static/pgp/server-8.0.asc | sudo gpg -o /usr/share/keyrings/mongodb-server-8.0.gpg --dearmor
echo "deb [ arch=amd64,arm64 signed-by=/usr/share/keyrings/mongodb-server-8.0.gpg ] https://repo.mongodb.org/apt/ubuntu $(lsb_release -cs)/mongodb-org/8.0 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-8.0.list
sudo apt update && sudo apt install -y mongodb-org
```
`$(lsb_release -cs)` fills in `noble` (24.04) or `jammy` (22.04) automatically.

Alternative: with Docker Desktop's WSL integration on, run `docker compose up -d` instead.

**3. Start MongoDB** (again each time you reopen WSL)
```bash
sudo systemctl start mongod          # if WSL has systemd enabled (default on new installs)
sudo systemctl status mongod
```
If `systemctl` says "System has not been booted with systemd", start it manually instead:
```bash
sudo mkdir -p /var/lib/mongodb /var/log/mongodb
sudo chown -R mongodb:mongodb /var/lib/mongodb /var/log/mongodb
sudo -u mongodb mongod --config /etc/mongod.conf --fork --logpath /var/log/mongodb/mongod.log
```
Test it: `mongosh --eval "db.runCommand({ ping: 1 })"` should print `ok: 1`.

**4. Put the project in the Linux file system and restore packages**
```bash
mkdir -p ~/projects && cd ~/projects
cp /mnt/c/Users/<YourWindowsName>/Downloads/MediTrack.Web-MongoDB.zip .
sudo apt install -y unzip && unzip MediTrack.Web-MongoDB.zip
cd MediTrack.Web
dotnet restore
```

**5. (Optional) Use a different connection string, e.g. MongoDB Atlas.** Keep it out of Git with user secrets:
```bash
dotnet user-secrets init
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb+srv://USER:PASSWORD@cluster0.xxxxx.mongodb.net/?serverSelectionTimeoutMS=5000"
```
For a local MongoDB the default in `appsettings.json` already works.

**6. Create the development HTTPS certificate**
```bash
dotnet dev-certs https
```
Your Windows browser will show a warning for it: click **Advanced → Continue to localhost**. Use HTTPS,
not HTTP: the login cookie is HTTPS-only.

**7. Run**
```bash
dotnet run
```
Open **https://localhost:7080** in your Windows browser and sign in:
- Email: `admin@meditrack.local`
- Password: `Admin@12345`

The demo user is created only in Development. Change the password in `appsettings.Development.json`.

**8. Check the data**
```bash
mongosh meditrack --eval 'db.users.find({}, { passwordHash: 0 }).pretty()'
```

## Note for later features (booking, pharmacy fulfilment)
MongoDB supports multi-document transactions only when running as a **replica set**. A plain local
`mongod` cannot run them. Before building booking (double-booking prevention) or stock deduction,
convert local MongoDB to a single-node replica set (`replication: replSetName: rs0` in `/etc/mongod.conf`,
restart, then `mongosh --eval 'rs.initiate()'`), and add `replicaSet=rs0` to the connection string.
MongoDB Atlas clusters are replica sets already.

## Login business rules (testable in AuthService)
| Case | Expected result |
|---|---|
| Correct email + password | Signed in, redirected to dashboard |
| Email in different case (`ADMIN@...`) | Treated as the same account |
| Wrong password | "Invalid email or password." (does not reveal which one was wrong) |
| 5 wrong passwords in a row | Account locked for 15 minutes |
| Inactive account | Sign-in refused |
| Empty or malformed email | Rejected client-side and server-side |
| `returnUrl` pointing to another site | Ignored, redirects to dashboard |
| Second account with the same email | Rejected by the unique index |

## Third-party packages
| Package | Purpose |
|---|---|
| MongoDB.Driver | MongoDB access from C# |
| Bootstrap 5.3 (CDN) | Layout and form styling |
