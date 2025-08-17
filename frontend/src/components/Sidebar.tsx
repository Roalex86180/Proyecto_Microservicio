import React from 'react';
import './Sidebar.css';
import { FaBars, FaAws, FaMicrosoft, FaGoogle, FaStar, FaProjectDiagram } from 'react-icons/fa';

interface SidebarProps {
  isSidebarVisible: boolean;
  toggleSidebar: () => void;
  onItemClick: (itemName: string) => void; // Nueva prop para manejar clics
}

const sidebarItems = [
  { name: 'Amazon Web Services', icon: <FaAws /> },
  { name: 'Microsoft Azure', icon: <FaMicrosoft /> },
  { name: 'Google Cloud', icon: <FaGoogle /> },
  { name: 'Testimonios', icon: <FaStar /> },
  { name: 'Estructura del Proyecto', icon: <FaProjectDiagram /> },
];

const Sidebar: React.FC<SidebarProps> = ({ isSidebarVisible, toggleSidebar, onItemClick }) => {
  return (
    <aside className={`sidebar ${!isSidebarVisible ? 'sidebar-hidden' : ''}`}>
      <div className="sidebar-toggle" onClick={toggleSidebar}>
        <FaBars />
      </div>
      <ul className="sidebar-list">
        {sidebarItems.map((item, index) => (
          <li key={index} onClick={() => onItemClick(item.name)}>
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