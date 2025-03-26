import React from 'react';
 
import Settings from '../components/Settings/Settings';
import Compliance from '../components/Compliance/Compliance';

export interface NavItem {
  path: string;
  label: string;
  emoji?: string; // Optional property for emoji
  element: React.ReactNode;
}

export const navItems: NavItem[] = [
  
  {
    path: '/Settings',
    label: 'Settings',
    emoji: '⚙️',
    element: <Settings />,
  },
  {
    path: '/Compliance',
    label: 'Compliance',
    emoji: '⚖️',
    element: <Compliance />,
  },
];
