// src/components/Sidebar.tsx
import React from 'react';
import './Sidebar.css';
import { FaBars, FaAws, FaMicrosoft, FaGoogle, FaStar, FaProjectDiagram } from 'react-icons/fa';

interface SidebarProps {
  isSidebarVisible: boolean;
  toggleSidebar: () => void;
}

const sidebarItems = [
  { name: 'Amazon Web Services', icon: <FaAws /> },
  { name: 'Microsoft Azure', icon: <FaMicrosoft /> },
  { name: 'Google Cloud', icon: <FaGoogle /> },
  { name: 'Testimonios', icon: <FaStar /> },
  { name: 'Estructura del Proyecto', icon: <FaProjectDiagram /> },
];

const Sidebar: React.FC<SidebarProps> = ({ isSidebarVisible, toggleSidebar }) => {
  return (
    <aside className={`sidebar ${!isSidebarVisible ? 'sidebar-hidden' : ''}`}>
      <div className="sidebar-toggle" onClick={toggleSidebar}>
        <FaBars />
      </div>
      <ul className="sidebar-list">
        {sidebarItems.map((item, index) => (
          <li key={index}>
            <a href="#">
              <span className="icon">{item.icon}</span>
              <span className="name">{item.name}</span>
            </a>
          </li>
        ))}
      </ul>
    </aside>
  );
};

export default Sidebar;