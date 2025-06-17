import React, { useState, ButtonHTMLAttributes } from 'react';
import { Search, Clock, TrendingUp, HelpCircle, Menu, X } from 'lucide-react';
import { NavItem } from '../types';

interface SideNavProps {
  activeItem: NavItem;
  onItemClick: (item: NavItem) => void;
}

const SideNav: React.FC<SideNavProps> = ({ activeItem, onItemClick }) => {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const navItems = [
    { id: 'search' as NavItem, label: 'Search Questions', icon: Search },
    { id: 'recent' as NavItem, label: 'Recent Questions', icon: Clock },
    { id: 'popular' as NavItem, label: 'Popular Today', icon: TrendingUp },
    { id: 'unanswered' as NavItem, label: 'Unanswered', icon: HelpCircle },
  ];

  const NavContent = () => (
    <nav className="h-full bg-white border-r border-[#E3E6E8]">
      <div className="p-4 border-b border-[#E3E6E8]">
        <div className="flex items-center space-x-3">
          <div className="p-2 bg-[#F48024] rounded">
            <Search className="w-5 h-5 text-white" />
          </div>
          <div>
            <h2 className="font-normal text-[#242729]">Stack Search</h2>
            <p className="text-xs text-[#6A737C]">Find answers fast</p>
          </div>
        </div>
      </div>
      
      <ul className="py-2">
        {navItems.map(({ id, label, icon: Icon }) => (
          <li key={id}>
            <button
              className={`w-full flex items-center px-4 py-2 text-left transition-all duration-200 ${
                activeItem === id
                  ? 'bg-[#F1F2F3] text-[#0C0D0E] border-r-4 border-[#F48024] font-normal'
                  : 'text-[#525960] hover:bg-[#F1F2F3] hover:text-[#0C0D0E]'
              }`}
              onClick={() => {
                onItemClick(id);
                setIsMobileMenuOpen(false);
              }}
            >
              <Icon className={`w-4 h-4 mr-2 ${activeItem === id ? 'text-[#F48024]' : 'text-[#525960]'}`} />
              {label}
            </button>
          </li>
        ))}
      </ul>
    </nav>
  );

  return (
    <>
      <button className="lg:hidden fixed top-4 left-4 z-50 p-2 bg-white rounded border border-[#E3E6E8]"
        onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
      >
        {isMobileMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
      </button>

      <div className="hidden lg:block w-64 h-full relative">
        <NavContent />
      </div>

      {isMobileMenuOpen && (
        <div className="lg:hidden fixed inset-0 z-40">
          <div className="absolute inset-0 bg-black bg-opacity-50" onClick={() => setIsMobileMenuOpen(false)} />
          <div className="relative w-64 h-full">
            <NavContent />
          </div>
        </div>
      )}
    </>
  );
};

export default SideNav;
