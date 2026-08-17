import { Navigate, Route, Routes } from "react-router-dom";
import { AppShell } from "./components/layout/AppShell";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { LoginPage } from "./pages/LoginPage";
import { RegisterPage } from "./pages/RegisterPage";
import { ForgotPasswordPage } from "./pages/ForgotPasswordPage";
import { ResetPasswordPage } from "./pages/ResetPasswordPage";
import { DashboardPage } from "./pages/DashboardPage";
import { TimesheetPage } from "./pages/TimesheetPage";
import { WorkShiftPage } from "./pages/WorkShiftPage";
import { LeavePage } from "./pages/LeavePage";
import { SafetyPage } from "./pages/SafetyPage";
import { IncidentsPage } from "./pages/IncidentsPage";
import { PoliciesPage } from "./pages/PoliciesPage";
import { AnnouncementsPage } from "./pages/AnnouncementsPage";
import { MedicalPage } from "./pages/MedicalPage";
import { PpePage } from "./pages/PpePage";
import { LegalAppointmentsPage } from "./pages/LegalAppointmentsPage";
import { ReportsPage } from "./pages/ReportsPage";
import { EmergencyPage } from "./pages/EmergencyPage";
import { SettingsPage } from "./pages/SettingsPage";
import { CertificatesPage } from "./pages/CertificatesPage";
import { KpiAppraisalsPage } from "./pages/KpiAppraisalsPage";
import { ComingSoonPage } from "./pages/ComingSoonPage";
import { NAV_ITEMS } from "./config/nav";

const BUILT_PATHS = new Set([
  "/",
  "/timesheet",
  "/work-shift",
  "/leave",
  "/safety-flra",
  "/incidents",
  "/policies",
  "/announcements",
  "/medical",
  "/ppe",
  "/legal-appointments",
  "/reports",
  "/emergency",
  "/settings",
  "/training",
  "/certificates",
  "/kpi",
]);

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password" element={<ResetPasswordPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<AppShell />}>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/timesheet" element={<TimesheetPage />} />
          <Route path="/work-shift" element={<WorkShiftPage />} />
          <Route path="/leave" element={<LeavePage />} />
          <Route path="/safety-flra" element={<SafetyPage />} />
          <Route path="/incidents" element={<IncidentsPage />} />
          <Route path="/policies" element={<PoliciesPage />} />
          <Route path="/announcements" element={<AnnouncementsPage />} />
          <Route path="/medical" element={<MedicalPage />} />
          <Route path="/ppe" element={<PpePage />} />
          <Route path="/legal-appointments" element={<LegalAppointmentsPage />} />
          <Route path="/reports" element={<ReportsPage />} />
          <Route path="/emergency" element={<EmergencyPage />} />
          <Route path="/settings" element={<SettingsPage />} />
          <Route path="/training" element={<CertificatesPage />} />
          <Route path="/certificates" element={<CertificatesPage />} />
          <Route path="/kpi" element={<KpiAppraisalsPage />} />
          {NAV_ITEMS.filter((item) => !BUILT_PATHS.has(item.path)).map(({ path, label }) => (
            <Route key={path} path={path} element={<ComingSoonPage title={label} />} />
          ))}
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;
