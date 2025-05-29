import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import SignIn from "./pages/AuthPages/SignIn";
import SignUp from "./pages/AuthPages/SignUp";
import NotFound from "./pages/OtherPage/NotFound";
import UserProfiles from "./pages/UserProfiles";

import Blank from "./pages/Blank";
import AppLayout from "./layout/AppLayout";
import { ScrollToTop } from "./components/common/ScrollToTop";
import Home from "./pages/Dashboard/Home";
import { AuthProvider } from "./context/AuthContext";
import { SidebarProvider } from "./context/SidebarContext";
import ProtectedRoute from "./components/auth/ProtectedRoute";
import Unauthorized from "./pages/Unauthorized";
import { ThemeProvider } from "./context/ThemeContext";
import { AppWrapper } from "./components/common/PageMeta";

// Brand Pages
import BrandsList from "./pages/Brands/BrandsList";
import BrandForm from "./pages/Brands/BrandForm";
import BrandDetails from "./pages/Brands/BrandDetails";

// Permissions
import { PERMISSIONS } from "./types/permissions";
import CategoryList from "./pages/Categorias/CategoryList";
import CategoryForm from "./pages/Categorias/CategoryForm";
import CategoryDetail from "./pages/Categorias/CategoryDetail";
import ProductList from "./pages/Products/ProductList";
import ProductForm from "./pages/Products/ProductForm";
import ProductDetail from "./pages/Products/productDetail";

// Componente interno que maneja las rutas
function AppRoutes() {
  return (
    <Routes>
      {/* Public Routes */}
      <Route path="/signin" element={<SignIn />} />
      <Route path="/signup" element={<SignUp />} />
      <Route path="/unauthorized" element={<Unauthorized />} />

      {/* Protected Routes with Dashboard Layout */}
      <Route element={
        <ProtectedRoute>
          <SidebarProvider>
            <AppLayout />
          </SidebarProvider>
        </ProtectedRoute>
      }>
        {/* Dashboard Home */}
        <Route index path="/" element={<Home />} />

        {/* Basic Pages */}
        <Route path="/profile" element={<UserProfiles />} />
        
        <Route path="/blank" element={<Blank />} />

        {/* Products Management */}
        <Route 
          path="/products" 
          element={
            <ProtectedRoute permissions={[PERMISSIONS.PRODUCTS.VIEW]}>
              <ProductList />
            </ProtectedRoute>
          } 
        />
        <Route path="/products/create" element={<ProtectedRoute permissions={[PERMISSIONS.PRODUCTS.CREATE]}><ProductForm /></ProtectedRoute>} />
        <Route path="/products/:id/edit" element={<ProtectedRoute permissions={[PERMISSIONS.PRODUCTS.EDIT]}><ProductForm /></ProtectedRoute>} />
        <Route path="/products/:id" element={<ProtectedRoute permissions={[PERMISSIONS.PRODUCTS.VIEW]}><ProductDetail /></ProtectedRoute>} />

        {/* Brands Management */}
        <Route 
          path="/brands" 
          element={
            <ProtectedRoute permissions={[PERMISSIONS.BRANDS.VIEW]}>
              <BrandsList />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/brands/create" 
          element={
            <ProtectedRoute permissions={[PERMISSIONS.BRANDS.CREATE]}>
              <BrandForm />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/brands/:id" 
          element={
            <ProtectedRoute permissions={[PERMISSIONS.BRANDS.VIEW]}>
              <BrandDetails />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/brands/:id/edit" 
          element={
            <ProtectedRoute permissions={[PERMISSIONS.BRANDS.EDIT]}>
              <BrandForm />
            </ProtectedRoute>
          } 
        />

        {/* Categories Management */}
        <Route 
          path="/categories" 
          element={
            <ProtectedRoute permissions={[PERMISSIONS.CATEGORIES.VIEW]}>
              <CategoryList />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/categories/create" 
          element={
            <ProtectedRoute permissions={[PERMISSIONS.CATEGORIES.CREATE]}>
              <CategoryForm />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/categories/:id" 
          element={
            <ProtectedRoute permissions={[PERMISSIONS.CATEGORIES.VIEW]}>
              <CategoryDetail />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/categories/:id/edit" 
          element={
            <ProtectedRoute permissions={[PERMISSIONS.CATEGORIES.EDIT]}>
              <CategoryForm />
            </ProtectedRoute> 
          } 
        />

        
        
      </Route>

      {/* Fallback Routes */}
      <Route path="/error-404" element={<NotFound />} />
      <Route path="*" element={<Navigate to="/error-404" replace />} />
    </Routes>
  );
}

export default function App() {
  return (
    <Router>
      <ScrollToTop />
      <AuthProvider>
        <ThemeProvider>
          <AppWrapper>
            <AppRoutes />
          </AppWrapper>
        </ThemeProvider>
      </AuthProvider>
    </Router>
  );
}