import Vue from "vue"
import Router from "vue-router"
import Home from "./views/Home.vue"

Vue.use(Router)

export let routes = [
    {
        path: "/",
        name: "Home",
        component: Home,
        icon: 'home',
    },
    // disable help until more features are added
    // {
    //     path: "/help",
    //     name: "help",
    //     // route level code-splitting
    //     // this generates a separate chunk (help.[hash].js) for this route
    //     // which is lazy-loaded when the route is visited.
    //     component: () => import(/* webpackChunkName: "about" */ "./views/Help.vue"),
    // },
    {
        path: "/debug",
        name: "Debug",
        icon: 'bug',
        // route level code-splitting
        // this generates a separate chunk (help.[hash].js) for this route
        // which is lazy-loaded when the route is visited.
        component: () => import(/* webpackChunkName: "about" */ "./views/Debug.vue"),
    },
    {
        path: "/gallery",
        name: "Gallery",
        icon: "picture-o",
        // route level code-splitting
        // this generates a separate chunk (help.[hash].js) for this route
        // which is lazy-loaded when the route is visited.
        component: () => import(/* webpackChunkName: "about" */ "./views/Gallery.vue"),
    },
    {
        path: "/activations",
        name: "Activations",
        icon: "calendar-check-o",
        // route level code-splitting
        // this generates a separate chunk (help.[hash].js) for this route
        // which is lazy-loaded when the route is visited.
        component: () => import(/* webpackChunkName: "about" */ "./views/Activations.vue"),
    },
    {
        path: "/people",
        name: "People",
        icon: "address-card",
        // route level code-splitting
        // this generates a separate chunk (help.[hash].js) for this route
        // which is lazy-loaded when the route is visited.
        component: () => import(/* webpackChunkName: "about" */ "./views/People.vue"),
    },
]

export default new Router({
    mode: "history",
    base: process.env.BASE_URL,
    routes: routes,
})
