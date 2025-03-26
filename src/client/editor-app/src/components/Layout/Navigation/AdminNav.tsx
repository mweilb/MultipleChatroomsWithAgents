// AdminNav.tsx
import React from 'react';
import { Link } from 'react-router-dom';

interface AdminNavProps {
  isCollapsed: boolean;
  navItems: any[];
  location: any;
}

const AdminNav: React.FC<AdminNavProps> = ({ isCollapsed, navItems, location }) => {
  const adminNavItems = navItems.filter(
    (item) => item.path === '/Settings' || item.path === '/Compliance'
  );

  return (
    <div className="nav-group">
      {!isCollapsed && <h3 className="group-label">Administration</h3>}
      <nav>
        <ul>
          {adminNavItems.map((item) => (
            <li key={item.path} className={location.pathname === item.path ? 'active' : ''}>
              <Link to={item.path}>
                {item.emoji ? <span className="nav-emoji">{item.emoji}</span> : item.icon}
                {!isCollapsed && <span>{item.label}</span>}
              </Link>
            </li>
          ))}
        </ul>
      </nav>
    </div>
  );
};

export default AdminNav;
