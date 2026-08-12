import { useEffect, useState, type FormEvent } from "react";
import { useAuth } from "../auth/AuthContext";
import { changePassword, getProfile } from "../api/account";
import { extractErrorMessage } from "../api/client";
import { EmployeeDirectory } from "../components/EmployeeDirectory";
import { SiteManagement } from "../components/SiteManagement";
import { EmployeeRole, EmployeeRoleLabels } from "../types/auth";
import type { ProfileDto } from "../types/account";

export function SettingsPage() {
  const { session } = useAuth();
  const isHR = session?.role === EmployeeRole.HR;
  const isExecutive = session?.role === EmployeeRole.Executive;

  const [profile, setProfile] = useState<ProfileDto | null>(null);
  const [profileError, setProfileError] = useState<string | null>(null);

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    getProfile()
      .then(setProfile)
      .catch((err) => setProfileError(extractErrorMessage(err)));
  }, []);

  async function handleChangePassword(event: FormEvent) {
    event.preventDefault();
    setPasswordError(null);
    setSuccessMessage(null);

    if (newPassword !== confirmPassword) {
      setPasswordError("New password and confirmation do not match.");
      return;
    }

    setIsSubmitting(true);
    try {
      await changePassword({ currentPassword, newPassword });
      setSuccessMessage("Password changed successfully.");
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
    } catch (err) {
      setPasswordError(extractErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="stagger-children flex flex-col gap-6">
      <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <h3 className="mb-4 text-sm font-semibold text-slate-900">My profile</h3>
        {profileError ? (
          <p className="text-sm text-red-600">{profileError}</p>
        ) : !profile ? (
          <p className="text-sm text-slate-500">Loading...</p>
        ) : (
          <dl className="grid grid-cols-1 gap-x-6 gap-y-3 sm:grid-cols-2">
            <div>
              <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">Full name</dt>
              <dd className="text-sm text-slate-900">{profile.fullName}</dd>
            </div>
            <div>
              <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">Email</dt>
              <dd className="text-sm text-slate-900">{profile.email}</dd>
            </div>
            <div>
              <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">Employee number</dt>
              <dd className="text-sm text-slate-900">{profile.employeeNumber}</dd>
            </div>
            <div>
              <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">Job title</dt>
              <dd className="text-sm text-slate-900">{profile.jobTitle}</dd>
            </div>
            <div>
              <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">Role</dt>
              <dd className="text-sm text-slate-900">{EmployeeRoleLabels[profile.role]}</dd>
            </div>
            <div>
              <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">Site</dt>
              <dd className="text-sm text-slate-900">{profile.siteName}</dd>
            </div>
          </dl>
        )}
      </div>

      <div className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <h3 className="mb-4 text-sm font-semibold text-slate-900">Change password</h3>
        <form onSubmit={handleChangePassword} className="flex max-w-sm flex-col gap-3">
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Current password
            <input
              type="password"
              required
              value={currentPassword}
              onChange={(e) => setCurrentPassword(e.target.value)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            New password
            <input
              type="password"
              required
              minLength={8}
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm text-slate-700">
            Confirm new password
            <input
              type="password"
              required
              minLength={8}
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-red-500 focus:outline-none focus:ring-2 focus:ring-red-500/15"
            />
          </label>

          {passwordError && <p className="text-sm text-red-600">{passwordError}</p>}
          {successMessage && <p className="text-sm text-emerald-600">{successMessage}</p>}

          <button
            type="submit"
            disabled={isSubmitting}
            className="mt-1 w-fit rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-500 disabled:opacity-50"
          >
            {isSubmitting ? "Changing..." : "Change password"}
          </button>
        </form>
      </div>

      {(isHR || isExecutive) && <SiteManagement canManage={isHR} />}
      {(isHR || isExecutive) && (
        <EmployeeDirectory canManage={isHR} canApproveRoleRequests={isHR || isExecutive} />
      )}
    </div>
  );
}
