import { useEffect, useRef } from "react";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { TOKEN_STORAGE_KEY, apiClient } from "../api/client";

function hubUrl(): string {
  const apiBaseUrl = apiClient.defaults.baseURL ?? "/api";
  return `${apiBaseUrl.replace(/\/api\/?$/, "")}/hubs/board`;
}

/** Joins the given board's SignalR group and calls onBoardUpdated whenever the server
 * broadcasts a change (task created/updated/status changed/deleted) — used to silently
 * refetch instead of tracking a live document diff. */
export function useBoardHub(boardId: string | undefined, onBoardUpdated: () => void) {
  const callbackRef = useRef(onBoardUpdated);
  callbackRef.current = onBoardUpdated;

  useEffect(() => {
    if (!boardId) return;

    const connection: HubConnection = new HubConnectionBuilder()
      .withUrl(hubUrl(), { accessTokenFactory: () => localStorage.getItem(TOKEN_STORAGE_KEY) ?? "" })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on("BoardUpdated", () => callbackRef.current());

    let isDisposed = false;
    connection
      .start()
      .then(() => {
        if (!isDisposed) return connection.invoke("JoinBoard", boardId);
      })
      .catch(() => {
        // Realtime is a convenience layer; the page still works via its own explicit refetches.
      });

    return () => {
      isDisposed = true;
      connection.stop();
    };
  }, [boardId]);
}
