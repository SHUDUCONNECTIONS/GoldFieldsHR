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
import { PoliciesPage } from "./pages/PoliciesPage";
import { AnnouncementsPage } from "./pages/AnnouncementsPage";
import { MedicalPage } from "./pages/MedicalPage";
import { PpePage } from "./pages/PpePage";
import { LegalAppointmentsPage } from "./pages/LegalAppointmentsPage";
import { ReportsPage } from "./pages/ReportsPage";
import { SettingsPage } from "./pages/SettingsPage";
import { CertificatesPage } from "./pages/CertificatesPage";
import { KpiHubPage } from "./pages/KpiHubPage";
import { KpiAppraisalsPage } from "./pages/KpiAppraisalsPage";
import { BoardsPage } from "./pages/BoardsPage";
import { BoardDetailPage } from "./pages/BoardDetailPage";
import { PerformancePage } from "./pages/PerformancePage";
import { ColleaguesPage } from "./pages/ColleaguesPage";
import { ComingSoonPage } from "./pages/ComingSoonPage";
import { NAV_ITEMS } from "./config/nav";

const BUILT_PATHS = new Set([
  "/",
  "/timesheet",
  "/work-shift",
  "/leave",
  "/policies",
  "/announcements",
  "/medical",
  "/ppe",
  "/legal-appointments",
  "/reports",
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
          <Route path="/policies" element={<PoliciesPage />} />
          <Route path="/announcements" element={<AnnouncementsPage />} />
          <Route path="/medical" element={<MedicalPage />} />
          <Route path="/ppe" element={<PpePage />} />
          <Route path="/legal-appointments" element={<LegalAppointmentsPage />} />
          <Route path="/reports" element={<ReportsPage />} />
          <Route path="/settings" element={<SettingsPage />} />
          <Route path="/training" element={<CertificatesPage />} />
          <Route path="/certificates" element={<CertificatesPage />} />
          <Route path="/kpi" element={<KpiHubPage />}>
            <Route index element={<KpiAppraisalsPage />} />
            <Route path="boards" element={<BoardsPage />} />
            <Route path="boards/:boardId" element={<BoardDetailPage />} />
            <Route path="performance" element={<PerformancePage />} />
            <Route path="colleagues" element={<ColleaguesPage />} />
          </Route>
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
