import { useCallback, useEffect, useRef, useState } from "react";
import { Download, Paperclip } from "lucide-react";
import { extractErrorMessage } from "../api/client";
import { downloadAttachment, getAttachmentsForEntity, uploadAttachment } from "../api/attachments";
import { useToast } from "./ToastProvider";
import { formatDateTime, formatFileSize } from "../lib/format";
import type { AttachmentDto, AttachmentEntityType } from "../types/attachment";

interface AttachmentsPanelProps {
  entityType: AttachmentEntityType;
  entityId: string;
  canUpload: boolean;
  compact?: boolean;
}

export function AttachmentsPanel({ entityType, entityId, canUpload, compact = false }: AttachmentsPanelProps) {
  const { showSuccess, showError } = useToast();
  const [attachments, setAttachments] = useState<AttachmentDto[]>([]);
  const [visible, setVisible] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [downloadingId, setDownloadingId] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const load = useCallback(async () => {
    try {
      const data = await getAttachmentsForEntity(entityType, entityId);
      setAttachments(data);
    } catch {
      // A 400 here means the viewer isn't authorized to see attachments on this record;
      // hide the panel entirely rather than show a confusing error on someone else's page.
      setVisible(false);
    }
  }, [entityType, entityId]);

  useEffect(() => {
    load();
  }, [load]);

  async function handleFileSelected(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    if (!file) return;
    setError(null);
    setIsUploading(true);
    try {
      await uploadAttachment(entityType, entityId, file);
      showSuccess(`"${file.name}" uploaded.`);
      await load();
    } catch (err) {
      const message = extractErrorMessage(err);
      setError(message);
      showError(message);
    } finally {
      setIsUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  }

  async function handleDownload(attachment: AttachmentDto) {
    setDownloadingId(attachment.id);
    setError(null);
    try {
      await downloadAttachment(attachment.id, attachment.fileName);
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setDownloadingId(null);
    }
  }

  if (!visible) return null;

  return (
    <div className={compact ? "" : "mt-3 border-t border-slate-100 pt-3"}>
      <div className="mb-2 flex items-center justify-between">
        <p className="flex items-center gap-1.5 text-xs font-medium uppercase tracking-wide text-slate-500">
          <Paperclip className="h-3.5 w-3.5" />
          {!compact && "Attachments"}
        </p>
        {canUpload && (
          <label className="cursor-pointer text-xs font-medium text-yellow-600 hover:underline">
            {isUploading ? "Uploading..." : "Upload file"}
            <input
              ref={fileInputRef}
              type="file"
              accept="application/pdf,image/jpeg,image/png"
              onChange={handleFileSelected}
              disabled={isUploading}
              className="hidden"
            />
          </label>
        )}
      </div>

      {error && <p className="mb-2 text-xs text-red-600">{error}</p>}

      {attachments.length === 0 ? (
        <p className="text-xs text-slate-400">No files attached.</p>
      ) : (
        <ul className="flex flex-col gap-1.5">
          {attachments.map((attachment) => (
            <li key={attachment.id} className="flex items-center justify-between gap-2 text-xs">
              <span className="truncate text-slate-700" title={attachment.fileName}>
                {attachment.fileName}
              </span>
              <span className="flex shrink-0 items-center gap-2 text-slate-400">
                {formatFileSize(attachment.sizeBytes)} · {attachment.uploadedByName} ·{" "}
                {formatDateTime(attachment.uploadedAtUtc)}
                <button
                  type="button"
                  disabled={downloadingId === attachment.id}
                  onClick={() => handleDownload(attachment)}
                  className="text-yellow-600 hover:underline disabled:opacity-50"
                >
                  <Download className="h-3.5 w-3.5" />
                </button>
              </span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
