import type { Metadata, Viewport } from 'next';
import './globals.css';
import { Geist } from "next/font/google";
import { cn } from "@/lib/utils";

const geist = Geist({subsets:['latin'],variable:'--font-sans'});

export const metadata: Metadata = {
  title: {
    default: 'ZEE',
    template: '%s · ZEE',
  },
  description: 'A global campus social network for verified students.',
  manifest: '/manifest.webmanifest',
  appleWebApp: {
    capable: true,
    title: 'ZEE',
    statusBarStyle: 'default',
  },
};

export const viewport: Viewport = {
  // ZEE is mobile-first and installable, so the viewport is pinned to the device
  // and themed for the OS chrome.
  width: 'device-width',
  initialScale: 1,
  themeColor: [
    { media: '(prefers-color-scheme: light)', color: '#ffffff' },
    { media: '(prefers-color-scheme: dark)', color: '#0b0b0f' },
  ],
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" className={cn("font-sans", geist.variable)}>
      <body className="min-h-dvh bg-white text-gray-900 antialiased dark:bg-gray-950 dark:text-gray-50">
        {children}
      </body>
    </html>
  );
}
