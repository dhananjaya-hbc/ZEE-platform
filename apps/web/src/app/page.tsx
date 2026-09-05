'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import type { ClipboardEvent, FormEvent, KeyboardEvent } from 'react';
import { useRouter } from 'next/navigation';
import { ApiError } from '@/lib/api-client';
import { requestOtp, verifyOtp } from '@/lib/auth';

type Step = 'email' | 'otp';

const CODE_LENGTH = 6;
const RESEND_COOLDOWN_SECONDS = 60;

/**
 * Sign-in: institutional email → one-time code → signed in.
 *
 * Shows one step at a time - email entry, then code entry replaces it once a code
 * has been sent. There is no live "is this domain valid" check as the student
 * types; the only real check is the request-otp call itself, and faking one before
 * that would be showing a checkmark for something never actually verified.
 */
export default function HomePage() {
  const router = useRouter();

  const [step, setStep] = useState<Step>('email');
  const [email, setEmail] = useState('');
  const [digits, setDigits] = useState<string[]>(Array(CODE_LENGTH).fill(''));
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [resendSecondsLeft, setResendSecondsLeft] = useState(0);

  const digitRefs = useRef<Array<HTMLInputElement | null>>([]);

  // Ticks the resend cooldown down once per second while it is running.
  useEffect(() => {
    if (resendSecondsLeft <= 0) {
      return;
    }

    const timer = setInterval(() => {
      setResendSecondsLeft((seconds) => Math.max(0, seconds - 1));
    }, 1000);

    return () => clearInterval(timer);
  }, [resendSecondsLeft]);

  /**
   * Turns a failure into copy a student can read. Deliberately does not
   * distinguish "wrong code" from "expired" from "already used" for a 400 from
   * verify-otp - see InvalidOtpException on the API: those are meant to look
   * identical to the caller.
   */
  const messageFor = useCallback((err: unknown): string => {
    if (err instanceof ApiError) {
      if (err.status === 404) {
        return "We don't recognise that institution yet. Check the address, or ask us to add your university.";
      }
      if (err.status === 429) {
        return 'Too many codes requested for this address. Please wait before trying again.';
      }
      return err.problem.detail ?? 'Something went wrong. Please try again.';
    }
    return 'Something went wrong. Please try again.';
  }, []);

  const sendCode = useCallback(async () => {
    setError(null);
    setIsSubmitting(true);

    try {
      await requestOtp(email);
      setStep('otp');
      setDigits(Array(CODE_LENGTH).fill(''));
      setResendSecondsLeft(RESEND_COOLDOWN_SECONDS);
      setTimeout(() => digitRefs.current[0]?.focus(), 0);
    } catch (err) {
      setError(messageFor(err));
    } finally {
      setIsSubmitting(false);
    }
  }, [email, messageFor]);

  function handleEmailSubmit(e: FormEvent) {
    e.preventDefault();
    void sendCode();
  }

  async function handleVerifySubmit(e: FormEvent) {
    e.preventDefault();

    const code = digits.join('');

    if (code.length !== CODE_LENGTH) {
      setError('Enter all 6 digits.');
      return;
    }

    setError(null);
    setIsSubmitting(true);

    try {
      await verifyOtp(email, code);
      router.push('/feed');
    } catch (err) {
      setError(messageFor(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  function handleDigitChange(index: number, value: string) {
    const digitsOnly = value.replace(/\D/g, '');

    setDigits((prev) => {
      const next = [...prev];
      next[index] = digitsOnly.length > 0 ? digitsOnly[digitsOnly.length - 1]! : '';
      return next;
    });

    if (digitsOnly && index < CODE_LENGTH - 1) {
      digitRefs.current[index + 1]?.focus();
    }
  }

  function handleDigitKeyDown(index: number, e: KeyboardEvent<HTMLInputElement>) {
    if (e.key === 'Backspace' && !digits[index] && index > 0) {
      digitRefs.current[index - 1]?.focus();
    }
  }

  function handleDigitPaste(e: ClipboardEvent<HTMLInputElement>) {
    const pasted = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, CODE_LENGTH);

    if (!pasted) {
      return;
    }

    e.preventDefault();

    const next = Array(CODE_LENGTH).fill('');
    for (let i = 0; i < pasted.length; i++) {
      next[i] = pasted[i];
    }
    setDigits(next);

    digitRefs.current[Math.min(pasted.length, CODE_LENGTH - 1)]?.focus();
  }

  return (
    <main className="mx-auto flex min-h-dvh max-w-md flex-col justify-center gap-6 px-6">
      <h1 className="text-3xl font-bold tracking-tight">ZEE</h1>

      {step === 'email' ? (
        <form onSubmit={handleEmailSubmit} className="flex flex-col gap-4">
          <div>
            <h2 className="text-xl font-semibold">Join with your university email</h2>
            <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
              Any verified institution, anywhere. We check your domain against the
              university register.
            </p>
          </div>

          <label className="flex flex-col gap-1 text-sm font-medium">
            University email
            <input
              type="email"
              required
              autoComplete="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@university.edu"
              className="rounded-md border border-gray-300 bg-gray-50 px-3 py-2 text-base focus:border-brand-500 focus:outline-none focus:ring-1 focus:ring-brand-500 dark:border-gray-700 dark:bg-gray-900"
            />
          </label>

          {error && <p className="text-sm text-red-600">{error}</p>}

          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-md bg-brand-600 px-4 py-2 font-semibold text-white transition hover:bg-brand-700 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isSubmitting ? 'Sending…' : 'Send code'}
          </button>

          <a
            href="mailto:hello@zee.example?subject=Add%20my%20university"
            className="text-center text-sm text-brand-600 underline"
          >
            Domain not listed? Request your university →
          </a>
        </form>
      ) : (
        <form onSubmit={(e) => void handleVerifySubmit(e)} className="flex flex-col gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-gray-500">
              Step 2 · Verify
            </p>
            <h2 className="text-xl font-semibold">Enter the 6-digit code</h2>
            <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">Sent to {email}.</p>
          </div>

          <div className="flex justify-between gap-2">
            {digits.map((digit, index) => (
              <input
                key={index}
                ref={(el) => {
                  digitRefs.current[index] = el;
                }}
                type="text"
                inputMode="numeric"
                autoComplete="one-time-code"
                maxLength={1}
                value={digit}
                onChange={(e) => handleDigitChange(index, e.target.value)}
                onKeyDown={(e) => handleDigitKeyDown(index, e)}
                onPaste={handleDigitPaste}
                className="h-14 w-12 rounded-md border border-gray-300 bg-gray-50 text-center text-xl focus:border-brand-500 focus:outline-none focus:ring-1 focus:ring-brand-500 dark:border-gray-700 dark:bg-gray-900"
              />
            ))}
          </div>

          <p className="text-sm text-gray-500">
            {resendSecondsLeft > 0 ? (
              `Resend in 0:${String(resendSecondsLeft).padStart(2, '0')}`
            ) : (
              <button
                type="button"
                onClick={() => void sendCode()}
                disabled={isSubmitting}
                className="text-brand-600 underline"
              >
                Resend code
              </button>
            )}
          </p>

          {error && <p className="text-sm text-red-600">{error}</p>}

          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-md bg-brand-600 px-4 py-2 font-semibold text-white transition hover:bg-brand-700 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isSubmitting ? 'Verifying…' : 'Verify'}
          </button>

          <button
            type="button"
            onClick={() => setStep('email')}
            className="text-center text-sm text-gray-500 underline"
          >
            Use a different email
          </button>
        </form>
      )}
    </main>
  );
}
