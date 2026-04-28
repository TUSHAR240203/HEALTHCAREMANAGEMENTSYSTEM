# Frontend role-based UI update

## What changed

- Added professional hospital dashboard shell with sidebar, glass UI cards, animations, and light/dark theme toggle.
- Added role-aware navigation for Admin, Receptionist, Doctor, and Patient.
- Added session role guard attribute to prevent opening modules outside the current role.
- Added JWT bearer forwarding from MVC session to gateway API calls.
- Added Admin staff-user management pages:
  - `/StaffUsers`
  - `/StaffUsers/Create`
- Added Doctor profile pages:
  - `/Doctors`
  - `/Doctors/Create`
  - `/Doctors/Details/{id}`
- Reworked Staff Login, Patient Login, Auth Preference, Profile, and Dashboard pages.
- Fixed patient login validation so it uses PatientId + MobileNumber + OTP only.

## Test path

1. Start API Gateway on `https://localhost:7000`.
2. Start Auth API, Patients API, Doctors API, Reception API, Appointments/Billing APIs if used.
3. Start MVC frontend.
4. Go to `/Account/StaffLogin`.
5. Login as admin by OTP or password.
6. Complete `/Account/AuthPreference` if first login.
7. Open `/StaffUsers` to create Doctor/Receptionist/Admin login accounts.
8. Open `/Doctors/Create` to create doctor professional profile.
9. Open `/Reception`, `/Patients`, `/Appointments`, `/Billing` based on role.

## Important backend dependency

Doctor Auth user and Doctor Profile are still two records:

- Auth API user = login/JWT/OTP/role.
- Doctors API profile = fullName, specialization, qualification, department, fee, license, etc.

Use the same mobile/email in both so they are easy to match.
