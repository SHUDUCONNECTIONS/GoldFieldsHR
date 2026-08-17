import { useEffect, useRef, useState, type FormEvent } from "react";
import { useAuth } from "../auth/AuthContext";
import { changePassword, getProfile, getSignature, setSignature } from "../api/account";
import { extractErrorMessage } from "../api/client";
import { EmployeeDirectory } from "../components/EmployeeDirectory";
import { SignaturePad, type SignaturePadHandle } from "../components/SignaturePad";
import { SiteManagement } from "../components/SiteManagement";
import { formatDateTime } from "../lib/format";
import { EmployeeRole, EmployeeRoleLabels } from "../types/auth";
import type { ProfileDto, SignatureDto } from "../types/account";

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

  const [signature, setSignatureState] = useState<SignatureDto | null>(null);
  const [signatureError, setSignatureError] = useState<string | null>(null);
  const [isSavingSignature, setIsSavingSignature] = useState(false);
  const signaturePadRef = useRef<SignaturePadHandle>(null);

  useEffect(() => {
    getProfile()
      .then(setProfile)
      .catch((err) => setProfileError(extractErrorMessage(err)));
    getSignature()
      .then(setSignatureState)
      .catch(() => {
        // Non-critical — the signature card just won't preload; the pad still works.
      });
  }, []);

  async function handleSaveSignature() {
    const dataUrl = signaturePadRef.current?.getSignature();
    if (!dataUrl) {
      setSignatureError("Draw your signature first.");
      return;
    }
    setSignatureError(null);
    setIsSavingSignature(true);
    try {
      const updated = await setSignature({ signaturePngBase64: dataUrl });
      setSignatureState(updated);
      signaturePadRef.current?.clear();
    } catch (err) {
      setSignatureError(extractErrorMessage(err));
    } finally {
      setIsSavingSignature(false);
    }
  }

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
          <p className="text-sm text-yellow-600">{profileError}</p>
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
        <h3 className="mb-1 text-sm font-semibold text-slate-900">My signature</h3>
        <p className="mb-4 text-xs text-slate-500">
          Used to sign policy acknowledgments and leave approvals. Draw it once here and it's reused automatically
          — redraw and save any time to update it.
        </p>

        {signature?.hasSignature && (
          <div className="mb-4 flex items-center gap-3 rounded-md border border-slate-200 bg-slate-50 p-3">
            <img src={signature.signaturePngBase64!} alt="Your saved signature" className="h-12 w-auto" />
            <p className="text-xs text-slate-500">
              Saved {signature.updatedAtUtc ? formatDateTime(signature.updatedAtUtc) : ""}
            </p>
          </div>
        )}

        <div className="max-w-md">
          <SignaturePad ref={signaturePadRef} height={140} />
        </div>

        {signatureError && <p className="mt-3 text-sm text-red-600">{signatureError}</p>}

        <button
          type="button"
          onClick={handleSaveSignature}
          disabled={isSavingSignature}
          className="mt-3 w-fit rounded-md bg-yellow-600 px-4 py-2 text-sm font-medium text-white hover:bg-yellow-500 disabled:opacity-50"
        >
          {isSavingSignature ? "Saving..." : signature?.hasSignature ? "Update signature" : "Save signature"}
        </button>
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
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
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
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
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
              className="rounded-md border border-slate-300 px-3 py-2 text-sm focus:border-yellow-500 focus:outline-none focus:ring-2 focus:ring-yellow-500/15"
            />
          </label>

          {passwordError && <p className="text-sm text-red-600">{passwordError}</p>}
          {successMessage && <p className="text-sm text-emerald-600">{successMessage}</p>}

          <button
            type="submit"
            disabled={isSubmitting}
            className="mt-1 w-fit rounded-md bg-yellow-600 px-4 py-2 text-sm font-medium text-white hover:bg-yellow-500 disabled:opacity-50"
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
