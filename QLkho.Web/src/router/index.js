import { createRouter, createWebHistory } from 'vue-router'
import DashboardView from '../views/DashboardView.vue'
import AssetsView from '../views/AssetsView.vue'
import EmployeesView from '../views/EmployeesView.vue'
import DepartmentsView from '../views/DepartmentsView.vue'
import CategoriesSuppliersView from '../views/CategoriesSuppliersView.vue'
import DocumentsView from '../views/DocumentsView.vue'
import LoginView from '../views/LoginView.vue'
import { isAuthenticated, getCurrentUser } from '../api/auth.js'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: { guestOnly: true },
    },
    {
      path: '/',
      name: 'dashboard',
      component: DashboardView,
      meta: { requiresAuth: true, roles: ['Admin'] },
    },
    {
      path: '/assets',
      name: 'assets',
      component: AssetsView,
      meta: { requiresAuth: true, roles: ['Admin'] },
    },
    {
      path: '/employees',
      name: 'employees',
      component: EmployeesView,
      meta: { requiresAuth: true, roles: ['Admin', 'HR'] },
    },
    {
      path: '/documents',
      name: 'documents',
      component: DocumentsView,
      meta: { requiresAuth: true, roles: ['Admin'] },
    },
    {
      path: '/departments',
      name: 'departments',
      component: DepartmentsView,
      meta: { requiresAuth: true, roles: ['Admin'] },
    },
    {
      path: '/categories-suppliers',
      name: 'categories-suppliers',
      component: CategoriesSuppliersView,
      meta: { requiresAuth: true, roles: ['Admin'] },
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/',
    },
  ],
})

// Navigation Guard kiểm tra phiên đăng nhập & Phân quyền Role (Admin vs HR)
router.beforeEach((to, from, next) => {
  const isAuth = isAuthenticated()
  const user = getCurrentUser()
  const userRole = user?.role || 'Admin'

  if (to.meta.requiresAuth) {
    if (!isAuth) {
      next({ name: 'login' })
      return
    }

    // Kiểm tra quyền truy cập theo Role
    if (to.meta.roles && !to.meta.roles.includes(userRole)) {
      // Nếu là HR và cố vào trang ngoài thẩm quyền -> Chuyển hướng về trang Quản lý Nhân sự
      if (userRole === 'HR') {
        next({ name: 'employees' })
        return
      }
      next({ name: 'dashboard' })
      return
    }

    next()
  } else if (to.meta.guestOnly && isAuth) {
    // Đã đăng nhập nhưng vào trang login -> Điều hướng đúng trang tương ứng với Role
    if (userRole === 'HR') {
      next({ name: 'employees' })
    } else {
      next({ name: 'dashboard' })
    }
  } else {
    next()
  }
})

export default router
