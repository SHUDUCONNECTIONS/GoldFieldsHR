let audioContext: AudioContext | null = null;

function getAudioContext(): AudioContext | null {
  if (typeof window === "undefined") return null;
  const AudioContextClass = window.AudioContext ?? (window as { webkitAudioContext?: typeof AudioContext }).webkitAudioContext;
  if (!AudioContextClass) return null;
  if (!audioContext) {
    audioContext = new AudioContextClass();
  }
  return audioContext;
}

/**
 * A sharp, alternating-pitch alarm beep — deliberately attention-grabbing
 * rather than a pleasant chime, so a new notification can't be missed.
 */
export function playNotificationSound(): void {
  const ctx = getAudioContext();
  if (!ctx) return;
  if (ctx.state === "suspended") {
    void ctx.resume();
  }

  const frequencies = [1046, 784, 1046];
  const beepDuration = 0.12;
  const gap = 0.05;

  frequencies.forEach((frequency, index) => {
    const startTime = ctx.currentTime + index * (beepDuration + gap);
    const oscillator = ctx.createOscillator();
    const gainNode = ctx.createGain();

    oscillator.type = "square";
    oscillator.frequency.setValueAtTime(frequency, startTime);

    gainNode.gain.setValueAtTime(0, startTime);
    gainNode.gain.linearRampToValueAtTime(0.35, startTime + 0.01);
    gainNode.gain.setValueAtTime(0.35, startTime + beepDuration - 0.02);
    gainNode.gain.linearRampToValueAtTime(0, startTime + beepDuration);

    oscillator.connect(gainNode);
    gainNode.connect(ctx.destination);

    oscillator.start(startTime);
    oscillator.stop(startTime + beepDuration);
  });
}
